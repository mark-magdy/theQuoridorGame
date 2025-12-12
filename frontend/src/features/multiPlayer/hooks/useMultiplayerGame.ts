import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/features/auth/context/AuthContext";
import { useGameHub } from "./useGameHub";
import { RoomDto } from "@/types/multiplayerTypes";
import { GameState, GameDto } from "@/types/gameTypes";
import {transformGameState} from "@/lib/utils" ;
export const useMultiplayerGame = () => {
  const router = useRouter();
  const { user } = useAuth();
  const [room, setRoom] = useState<RoomDto | null>(null);
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [isGameStarted, setIsGameStarted] = useState(false);
  const [copySuccess, setCopySuccess] = useState(false);

  const { isConnected, error, startGame, leaveRoom, makeMove, rejoinRoom } = useGameHub({
    currentRoomId: room?.roomId,
    onGameStarted: (gameDto) => {
      console.log("Game started:", gameDto);
      setGameState(transformGameState(gameDto.gameState));
      setIsGameStarted(true);
      // Save game state to sessionStorage
      sessionStorage.setItem("currentGameState", JSON.stringify(transformGameState(gameDto.gameState)));
    },
    onGameStateUpdated: (gameDto) => {
      console.log("Game state updated:", gameDto);
      setGameState(transformGameState(gameDto.gameState));
      sessionStorage.setItem("currentGameState", JSON.stringify(transformGameState(gameDto.gameState)));
    },
    onGameEnded: (gameDto) => {
      console.log("Game ended:", gameDto);
      setGameState(transformGameState(gameDto.gameState));
      sessionStorage.setItem("currentGameState", JSON.stringify(transformGameState(gameDto.gameState)));
    },
    onRoomUpdated: (updatedRoom) => {
      console.log("Room updated:", updatedRoom);
      setRoom(updatedRoom);
      // Update sessionStorage with latest room state
      sessionStorage.setItem("currentRoom", JSON.stringify(updatedRoom));
      
      // If room status changed to InProgress, update isGameStarted
      if (updatedRoom.status === "InProgress" && !isGameStarted) {
        setIsGameStarted(true);
      }
    },
    onRoomClosed: () => {
      alert("Room has been closed");
      sessionStorage.removeItem("currentRoom");
      router.push("/");
    },
    onReconnected: async () => {
      console.log("Reconnected to server, fetching latest room state");
      if (room) {
        try {
          const updatedRoom = await rejoinRoom(room.roomId);
          if (updatedRoom) {
            setRoom(updatedRoom);
            sessionStorage.setItem("currentRoom", JSON.stringify(updatedRoom));
          }
        } catch (err) {
          console.error("Failed to sync room after reconnection:", err);
        }
      }
    },
  });

  useEffect(() => {
    // Check if we're in the browser
    if (typeof window === "undefined") return;
    
    // Get room data from sessionStorage (set when creating/joining room)
    const roomData = sessionStorage.getItem("currentRoom");
    if (roomData) {
      try {
        const parsedRoom = JSON.parse(roomData);
        setRoom(parsedRoom);
        
        // Check if there's an ongoing game
        if (parsedRoom.status === "InProgress") {
          const savedGameState = sessionStorage.getItem("currentGameState");
          console.log(" <> savedGameState:", savedGameState);
          if (savedGameState) {
            try {
              setGameState(JSON.parse(savedGameState));
              setIsGameStarted(true);
            } catch (err) {
              console.error("Failed to parse game state:", err);
            }
          }
        }
      } catch (err) {
        console.error("Failed to parse room data:", err);
        router.push("/");
      }
    } else {
      router.push("/");
    }
  }, [router]);

  const handleStartGame = async () => {
    if (!room) return;
    try {
      await startGame(room.roomId);
    } catch (err) {
      alert("Failed to start game: " + (err instanceof Error ? err.message : "Unknown error"));
    }
  };

  const handleLeaveRoom = async () => {
    if (!room) return;
    try {
      await leaveRoom(room.roomId);
      sessionStorage.removeItem("currentRoom");
      router.push("/");
    } catch (err) {
      console.error("Failed to leave room:", err);
      router.push("/");
    }
  };

  const handleCopyRoomId = () => {
    if (room) {
      navigator.clipboard.writeText(room.roomId);
      setCopySuccess(true);
      setTimeout(() => setCopySuccess(false), 2000);
    }
  };

  const handlePawnMove = async (position: { row: number; col: number }) => {
    if (!room) return;
    try {
      await makeMove({
        roomId: room.roomId,
        moveType: "move",
        toRow: position.row,
        toCol: position.col,
      });
    } catch (err) {
      console.error("Move failed:", err);
    }
  };

  const handleWallPlace = async (wall: { position: { row: number; col: number }; isHorizontal: boolean }) => {
    if (!room) return false;
    try {
      await makeMove({
        roomId: room.roomId,
        moveType: "wall",
        toRow: wall.position.row,
        toCol: wall.position.col,
        wallOrientation: wall.isHorizontal ? "horizontal" : "vertical",
      });
      return true;
    } catch (err) {
      console.error("Wall placement failed:", err);
      return false;
    }
  };

  const isHost = user && room?.players.find((p) => p.isHost)?.userId === user.id;
  const canStartGame = isHost && room && room.currentPlayers >= 2 && !isGameStarted;
  
  // Determine if current user is a player in the game
  const currentUserPlayer = gameState?.players.find((p) => p.userId === user?.id);
  const isCurrentPlayerTurn = currentUserPlayer && gameState?.currentPlayerIndex === currentUserPlayer.id;
  const isSpectator = isGameStarted && !currentUserPlayer;

  return {
    room,
    gameState,
    isGameStarted,
    isConnected,
    error,
    copySuccess,
    isHost,
    canStartGame,
    isCurrentPlayerTurn,
    isSpectator,
    currentUserPlayer,
    handleStartGame,
    handleLeaveRoom,
    handleCopyRoomId,
    handlePawnMove,
    handleWallPlace,
  };
};
