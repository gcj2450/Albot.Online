using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game {

	// It's in general unadvised to have abstract classes 
	// with data members. Is this something we have to 
	// rethink?  - Joel Wretborn

	// No, never! - Fredrik Carlsson
	public abstract class GameMaster {
		protected GameWrapper wrapper;
		protected List<ConnectedPlayer> players = new List<ConnectedPlayer>();
		protected List<ConnectedClient> clients = new List<ConnectedClient>();
		protected Action<string> print;
		protected Action shutdownGameServer;
		protected List<string> preGamePlayers;
		protected List<Game.PlayerColor> colorOrder;
		protected List<string> gameHistory = new List<string>();
		protected GameHistory gameLog = new GameHistory ();

		public abstract GameType getGameType ();
		public GameHistory getGameHistory(){return gameLog;}
		public GameMaster getInstance(){return this;}
		public abstract CommProtocol getProtocol();
		public abstract bool isRealtime ();
		protected abstract void initProtocol( Action<object, int, short> sendMsgFunc);
		public int nbrPlayers() { return players.Count; }

		public void init(Action<string> printFunc, Action<object, int, short> sendMsgFunc, Action shutdownGameServer,GameWrapper wrapper, List<string> preGamePlayers){
			print = printFunc;
			initProtocol(sendMsgFunc);
			this.wrapper = wrapper;
			this.shutdownGameServer = shutdownGameServer;
			this.preGamePlayers = preGamePlayers;

			print("Amount of Players : " + preGamePlayers.Count);
			foreach (string s in preGamePlayers)
				print ("Player: " + s);
		}




		//Currently we have no way of knowing if the same player is registred twice...
		public void addNewPlayerAndClient(ConnectedPlayer p){
			if (clients.Any (x => x.peerID == p.client.peerID) == false)
				clients.Add (p.client);

			players.Add (p);
		}

		public int getPreGamePlayersIndex(string username){return preGamePlayers.FindIndex (x => x == username);}
		public PlayerColor assignPlayerColor(string username, List<PlayerColor> colors){
			int index = getPreGamePlayersIndex(username);
			if (index >= 0)
				return colors [index];
			else {
				print ("Did not find matching color for: " + username);
				return PlayerColor.None;
			}
		}

		//******************************************************
		//Later add so only admins can shutdown the server
		protected void shutdownServer(){
			print ("Got a shutdown msg");
			shutdownGameServer ();
		}

	    public abstract void startGame();
		public virtual ConnectedPlayer onPlayerJoined(ConnectedPlayer newPlayer){
			addNewPlayerAndClient (newPlayer);
			newPlayer.color = assignPlayerColor (newPlayer.username, colorOrder);
			return newPlayer;
		}
		public abstract void onPlayerLeft(ConnectedPlayer newPlayer);
	    public abstract int maxNbrPlayers();
		public ConnectedPlayer getMatchingPlayer(Game.PlayerColor color){return players.Find (x => x.color == color);}
		public List<ConnectedPlayer> getClientPlayers(ConnectedClient c){return players.FindAll (x => x.client.peerID == c.peerID);}



		#region Spectator mode
		public byte[] getCurrentLog(){
			return gameLog.getCurrentLog ();
		}
		#endregion
	}   

	public enum PlayerColor{
		Red, Green, Blue, Yellow, Black, White, None 
	}
		
	public enum GameType{
		None,
		Connect4,
		BumtTag,
		Othello,
		Chess,
		FallingDebris,
		Battleship,
		Soldiers,
		BrickPuzzle,
		Game2048,
		TowerOfHanoi,
		Bomberman,
		Breakthrough,
		Snake,
		HeapManager,
		FileManager,
		BlockBattle
	}

	public class GameUtil{
		public static GameType stringToGameType(string t){
			switch (t.ToUpper()) {

			case "CONNECT4":return GameType.Connect4;
			case "BUMPTAG":return GameType.BumtTag;
			case "OTHELLO":return GameType.Othello;
			case "CHESS":return GameType.Chess;
			case "FALLINGDEBRIS":return GameType.FallingDebris;
			case "BATTLESHIP":return GameType.Battleship;
			case "SOLDIERS":return GameType.Soldiers;
			case "BRICKPUZZLE":return GameType.BrickPuzzle;
			case "GAME2048":return GameType.Game2048;
			case "TOWEROFHANOI":return GameType.TowerOfHanoi;
			case "BOMBERMAN":return GameType.Bomberman;
			case "BREAKTHROUGH":return GameType.Breakthrough;
			case "SNAKE":return GameType.Snake;	
			case "BLOCKBATTLE":return GameType.BlockBattle;	
				
			default: return GameType.None;
			}
		}
	}

} // namespace Game