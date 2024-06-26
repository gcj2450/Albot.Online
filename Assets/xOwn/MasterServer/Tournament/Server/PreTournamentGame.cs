﻿using Barebones.MasterServer;
using Barebones.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AlbotDB;
using AlbotServer;

namespace Tournament.Server {

    public class PreTournamentGame {

        private TournamentGameInfo gameInfo;
  

        public PreTournamentGame(TournamentInfoMsg specs, IPeer admin, string roomID) {
            specs.tournamentID = roomID;
            gameInfo = new TournamentGameInfo() {
                admin = admin, roomID = roomID, specs = specs,
                connectedPeers = new List<IPeer>(),
                players = new List<TournamentPlayer>(),
                hostName = "Admin",
                doubleElimination = specs.doubleElimination
            };
        }

        public bool addPlayer(IPeer peer, PlayerInfo info) {
            if (gameInfo.players.Count >= gameInfo.specs.maxPlayers || containsPeer(peer))
                return false;

            peer.Disconnected += playerDissconnected;
            gameInfo.connectedPeers.Add(peer);
            gameInfo.players.Add(new TournamentPlayer(false) { info = info, peer = peer });
            updateAdmin();
            return true;
        }

        private void playerDissconnected(IPeer peer) {
            Debug.LogError("Peer dissconnected: " + peer.Id);
            removePlayer(peer.Id);
        }
        public void removePlayer(int peerID) {
            IPeer localPeer = getMachingPeer(peerID);
            localPeer.Disconnected -= playerDissconnected;

            gameInfo.connectedPeers.Remove(localPeer);
            gameInfo.players.RemoveAll(p => p.peer.Id == peerID);
            updateAdmin();
        }

        public bool startTournament(ref RunningTournamentGame game, IIncommingMessage rawMsg) {
            if (gameInfo.connectedPeers.Count == 0) {
                rawMsg.Respond("Tried to start tournament, but there was not enough players!", ResponseStatus.Failed);
                return false;
            }

            gameInfo.connectedPeers.ForEach(p => p.Disconnected -= playerDissconnected);
            game = new RunningTournamentGame(gameInfo, ServerUtils.tournamentInfoToGameSpecs(gameInfo.specs), gameInfo.doubleElimination);
            gameInfo.connectedPeers.ForEach(p => p.SendMessage((short)CustomMasterServerMSG.startTournament, gameInfo.specs));
            rawMsg.Respond(gameInfo.specs, ResponseStatus.Success);
            return true;
        }

        public void closeTournament() {
            Debug.LogError("Closing tournament: " + gameInfo.roomID);
            gameInfo.connectedPeers.ForEach(p => p.SendMessage((short)CustomMasterServerMSG.closeTournament, gameInfo.roomID));
        }

        public void updateAdmin() {
            TournamentInfoMsg infoMsg = new TournamentInfoMsg() {players = gameInfo.players.Select(p => p.info).ToArray()};
            gameInfo.admin.SendMessage((short)CustomMasterServerMSG.preTournamentUpdate, infoMsg);
        }


        #region Compression
        public GameInfoPacket convertToGameInfoPacket() {
            return Msf.Helper.createGameInfoPacket(GameInfoType.PreTournament, gameInfo.roomID, gameInfo.hostName, gameInfo.specs.maxPlayers, gameInfo.players.Count, gameInfo.specs.type);
        }
        #endregion

        public string getRoomID() { return gameInfo.roomID; }
        public int[] getPeerIDs() { return gameInfo.connectedPeers.Select(p => p.Id).ToArray(); }
        public IPeer getMachingPeer(int id) {return gameInfo.connectedPeers.First(p => p.Id == id); }
        public bool containsPeer(IPeer peer) { return gameInfo.connectedPeers.Any(p => p.Id == peer.Id); }
        public IPeer getAdmin() { return gameInfo.admin; }
    }

}