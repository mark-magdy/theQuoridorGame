import { useEffect, useRef, useState, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import { RoomDto, GameMoveDto, ChatMessage } from "@/types/multiplayerTypes";
import { GameState, GameDto } from "@/types/gameTypes";
import {transformGameState} from "@/lib/utils"; 
interface UseGameHubOptions {
  currentRoomId?: string; // Room to rejoin on reconnection
  onRoomUpdated?: (room: RoomDto) => void; // Replaces onPlayerJoined/onPlayerLeft
  onPlayerReconnected?: (data: { userId: string; room: RoomDto }) => void;
  onRoomClosed?: () => void;
  onGameStarted?: (gameDto: GameDto) => void;
  onGameStateUpdated?: (gameDto: GameDto) => void; // Replaces onMoveMade
  onGameEnded?: (gameDto: GameDto) => void;
  onChatMessage?: (message: ChatMessage) => void;
  onReconnected?: () => void;
}

export const useGameHub = (options: UseGameHubOptions = {}) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const optionsRef = useRef(options);

  // Keep options ref up to date
  useEffect(() => {
    optionsRef.current = options;
  }, [options]);

  useEffect(() => {
    // Check if we're in the browser
    if (typeof window === "undefined") return;
    
    // Get JWT token from localStorage - must match the key used in apiClient
    const token = localStorage.getItem("auth_token");
    if (!token) {
      setError("Not authenticated - please sign in");
      return;
    }

    const hubUrl = process.env.NEXT_PUBLIC_SIGNALR_HUB_URL ;

    // Create SignalR connection
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl!, {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0, 2, 10, 30 seconds
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connectionRef.current = newConnection;
    setConnection(newConnection);

    // Handle reconnection
    newConnection.onreconnected(async () => {
      console.log("[SignalR] Reconnected successfully");
      setIsConnected(true);
      
      // Rejoin room if we were in one
      if (optionsRef.current.currentRoomId) {
        try {
          console.log(`[SignalR] Rejoining room ${optionsRef.current.currentRoomId} after reconnection`);
          const room = await newConnection.invoke<RoomDto | null>("RejoinRoom", optionsRef.current.currentRoomId);
          if (room) {
            console.log("[SignalR] Successfully rejoined room:", room);
            optionsRef.current.onRoomUpdated?.(room);
          }
          optionsRef.current.onReconnected?.();
        } catch (err) {
          console.error("[SignalR] Error rejoining room after reconnection:", err);
        }
      }
    });

    newConnection.onreconnecting((error) => {
      console.log("[SignalR] Connection lost, reconnecting...", error);
      setIsConnected(false);
    });

    newConnection.onclose((error) => {
      console.log("[SignalR] Connection closed", error);
      setIsConnected(false);
    });

    // Set up event handlers - using arrow functions to access latest optionsRef
    newConnection.on("RoomUpdated", (room: RoomDto) => {
      console.log("[SignalR] Room updated:", room);
      optionsRef.current.onRoomUpdated?.(room);
    });

    newConnection.on("PlayerReconnected", (data: { userId: string; room: RoomDto }) => {
      console.log("[SignalR] Player reconnected:", data);
      optionsRef.current.onPlayerReconnected?.(data);
      optionsRef.current.onRoomUpdated?.(data.room);
    });

    newConnection.on("RoomClosed", () => {
      console.log("[SignalR] Room closed");
      optionsRef.current.onRoomClosed?.();
    });

    newConnection.on("GameStarted", (gameDto: GameDto) => {
      console.log("[SignalR] Game started:", gameDto);
      optionsRef.current.onGameStarted?.(gameDto);
    });

    newConnection.on("GameStateUpdated", (gameDto: GameDto) => {
      console.log("[SignalR] Game state updated:", gameDto);
      optionsRef.current.onGameStateUpdated?.(gameDto);
    });

    newConnection.on("GameEnded", (gameDto: GameDto) => {
      console.log("[SignalR] Game ended:", gameDto);
      optionsRef.current.onGameEnded?.(gameDto);
    });

    newConnection.on("ChatMessage", (message: ChatMessage) => {
      console.log("[SignalR] Chat message:", message);
      optionsRef.current.onChatMessage?.(message);
    });

    // Start connection
    const startConnection = async () => {
      try {
        await newConnection.start();
        console.log("[SignalR] Connected successfully");
        setIsConnected(true);
        setError(null);
        
        // Auto-rejoin room if specified
        if (optionsRef.current.currentRoomId) {
          try {
            console.log(`[SignalR] Auto-rejoining room ${optionsRef.current.currentRoomId} on initial connect`);
            const room = await newConnection.invoke<RoomDto | null>("RejoinRoom", optionsRef.current.currentRoomId);
            if (room) {
              console.log("[SignalR] Auto-rejoin successful:", room);
              optionsRef.current.onRoomUpdated?.(room);
            }
          } catch (err) {
            console.error("[SignalR] Error auto-rejoining room:", err);
          }
        }
      } catch (err) {
        console.error("[SignalR] Connection error:", err);
        setError(err instanceof Error ? err.message : "Connection failed");
        setIsConnected(false);
      }
    };

    startConnection();

    // Cleanup
    return () => {
      console.log("[SignalR] Cleaning up connection");
      if (connectionRef.current) {
        connectionRef.current.stop().catch((err) => {
          console.error("[SignalR] Error stopping connection:", err);
        });
      }
    };
  }, []); // Empty deps - only run once on mount

  const createRoom = useCallback(
    async (maxPlayers: number = 2): Promise<RoomDto> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log(`[SignalR] Creating room with ${maxPlayers} max players`);
        const room = await connection.invoke<RoomDto>("CreateRoom", { maxPlayers });
        console.log("[SignalR] Room created:", room);
        return room;
      } catch (err) {
        console.error("[SignalR] Error creating room:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  const joinRoom = useCallback(
    async (roomId: string): Promise<RoomDto> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log(`[SignalR] Joining room: ${roomId}`);
        const room = await connection.invoke<RoomDto>("JoinRoom", { roomId });
        console.log("[SignalR] Joined room:", room);
        return room;
      } catch (err) {
        console.error("[SignalR] Error joining room:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  const leaveRoom = useCallback(
    async (roomId: string): Promise<void> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log(`[SignalR] Leaving room: ${roomId}`);
        await connection.invoke("LeaveRoom", roomId);
        console.log("[SignalR] Left room successfully");
      } catch (err) {
        console.error("[SignalR] Error leaving room:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  const startGame = useCallback(
    async (roomId: string): Promise<void> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log(`[SignalR] Starting game in room: ${roomId}`);
        await connection.invoke("StartGame", roomId);
        console.log("[SignalR] Game start request sent");
      } catch (err) {
        console.error("[SignalR] Error starting game:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  const makeMove = useCallback(
    async (moveDto: GameMoveDto): Promise<void> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log("[SignalR] Making move:", moveDto);
        await connection.invoke("MakeMove", moveDto);
        console.log("[SignalR] Move sent successfully");
      } catch (err) {
        console.error("[SignalR] Error making move:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  const sendChatMessage = useCallback(
    async (roomId: string, message: string): Promise<void> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log(`[SignalR] Sending chat message to room ${roomId}:`, message);
        await connection.invoke("SendChatMessage", roomId, message);
      } catch (err) {
        console.error("[SignalR] Error sending chat message:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  const rejoinRoom = useCallback(
    async (roomId: string): Promise<RoomDto | null> => {
      if (!connection || !isConnected) {
        throw new Error("Not connected to game server");
      }

      try {
        console.log(`[SignalR] Rejoining room: ${roomId}`);
        const room = await connection.invoke<RoomDto | null>("RejoinRoom", roomId);
        console.log("[SignalR] Rejoin result:", room);
        return room;
      } catch (err) {
        console.error("[SignalR] Error rejoining room:", err);
        throw err;
      }
    },
    [connection, isConnected]
  );

  return {
    isConnected,
    error,
    createRoom,
    joinRoom,
    rejoinRoom,
    leaveRoom,
    startGame,
    makeMove,
    sendChatMessage,
  };
};