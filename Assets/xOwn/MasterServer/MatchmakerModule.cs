﻿using System.Collections.Generic;
using System.Linq;
using Barebones.Networking;
using UnityEngine;
using AlbotServer;

namespace Barebones.MasterServer{
	
    public class MatchmakerModule : ServerModuleBehaviour{
		
        protected HashSet<IGamesProvider> GameProviders;

        protected virtual void Awake(){
            AddOptionalDependency<RoomsModule>();
            AddOptionalDependency<LobbiesModule>();
        }

        public override void Initialize(IServer server){
            base.Initialize(server);
            GameProviders = new HashSet<IGamesProvider>();

            if (server.GetModule<RoomsModule>() != null)
                AddProvider(server.GetModule<RoomsModule>());

            if (server.GetModule<LobbiesModule>() != null)
                AddProvider(server.GetModule<LobbiesModule>());

            server.SetHandler((short) MsfOpCodes.FindGames, HandleFindGames);
        }

        public void AddProvider(IGamesProvider provider){
            GameProviders.Add(provider);
        }

		//For now we only show pre games!
        private void HandleFindGames(IIncommingMessage message){
            var list = new List<GameInfoPacket>();
            var filters = new Dictionary<string, string>().FromBytes(message.AsBytes());

			/*
            foreach (var provider in GameProviders)
                list.AddRange(provider.GetPublicGames(message.Peer, filters));
			*/

			foreach (PreGame p in AlbotPreGameModule.singleton.currentPreGames)
				list.Add (p.convertToGameInfoPacket ());

            // Convert to generic list and serialize to bytes
            var bytes = list.Select(l => (ISerializablePacket)l).ToBytes();
            message.Respond(bytes, ResponseStatus.Success);

			int currentGamesCounter = 0;
			foreach (var provider in GameProviders)
				currentGamesCounter += provider.GetPublicGames (message.Peer, filters).ToList().Count;
			message.Peer.SendMessage ((short)ServerCommProtocl.LobbyGameStats, new LobbyGameStatsMsg () {currentActiveGames = currentGamesCounter, totalGamesPlayed = GamesData.totallGamesPlayed});
        }

		//Sends a list of all the current games that are either being played or in lobby
		public List<GameInfoPacket> getCurrentSpectatorGames(IPeer p){
			List<GameInfoPacket> gameList = new List<GameInfoPacket>();

			foreach (var provider in GameProviders)
				gameList.AddRange(provider.GetPublicGames(p, new Dictionary<string, string>()));
			foreach (PreGame g in AlbotPreGameModule.singleton.currentPreGames)
				gameList.Add (g.convertToGameInfoPacket ());

			return gameList;
		}
    }
}