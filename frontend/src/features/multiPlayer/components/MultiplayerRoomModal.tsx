"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import Modal from "@/components/ui/Modal";
import Button from "@/components/common/Button";
import { useGameHub } from "@/features/multiPlayer/hooks/useGameHub";

interface MultiplayerRoomModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const MultiplayerRoomModal = ({ isOpen, onClose }: MultiplayerRoomModalProps) => {
  const router = useRouter();
  const [mode, setMode] = useState<"select" | "create" | "join">("select");
  const [maxPlayers, setMaxPlayers] = useState(2);
  const [roomId, setRoomId] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [connectionStatus, setConnectionStatus] = useState<"connecting" | "connected" | "error">("connecting");

  const { isConnected, error: connectionError, createRoom, joinRoom } = useGameHub();

  useEffect(() => {
    if (isOpen) {
      if (isConnected) {
        setConnectionStatus("connected");
        setError(null);
      } else if (connectionError) {
        setConnectionStatus("error");
        setError(connectionError);
      } else {
        setConnectionStatus("connecting");
      }
    }
  }, [isOpen, isConnected, connectionError]);

  const handleCreateRoom = async () => {
    if (!isConnected) {
      setError("Not connected to game server. Please wait...");
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const room = await createRoom(maxPlayers);
      // Store room data in sessionStorage
      sessionStorage.setItem("currentRoom", JSON.stringify(room));
      // Navigate to game page
      router.push("/MultiPlayerGame");
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create room");
    } finally {
      setIsLoading(false);
    }
  };

  const handleJoinRoom = async () => {
    if (!roomId.trim()) {
      setError("Please enter a room ID");
      return;
    }

    if (!isConnected) {
      setError("Not connected to game server. Please wait...");
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const room = await joinRoom(roomId.toUpperCase());
      // Store room data in sessionStorage
      sessionStorage.setItem("currentRoom", JSON.stringify(room));
      // Navigate to game page
      router.push("/MultiPlayerGame");
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to join room");
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setMode("select");
    setRoomId("");
    setError(null);
    setConnectionStatus("connecting");
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Play with a Friend">
      <div className="space-y-4">
        {/* Connection Status */}
        {connectionStatus === "connecting" && (
          <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
            <div className="flex items-center gap-3">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
              <p className="text-sm text-blue-800 dark:text-blue-200">
                Connecting to game server...
              </p>
            </div>
          </div>
        )}

        {connectionStatus === "error" && (
          <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
            <p className="text-sm text-red-800 dark:text-red-200">
              Connection error: {error}
            </p>
          </div>
        )}

        {connectionStatus === "connected" && (
          <>
            {mode === "select" && (
              <div className="space-y-3">
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
                  Choose an option to start playing with friends
                </p>
                <Button
                  onClick={() => setMode("create")}
                  className="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white"
                >
                  Create New Room
                </Button>
                <Button
                  onClick={() => setMode("join")}
                  className="w-full py-3 bg-green-600 hover:bg-green-700 text-white"
                >
                  Join Existing Room
                </Button>
              </div>
            )}

            {mode === "create" && (
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">
                    Number of Players
                  </label>
                  <select
                    value={maxPlayers}
                    onChange={(e) => setMaxPlayers(Number(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                  >
                    <option value={2}>2 Players</option>
                    <option value={3} disabled>3 Players (Coming Soon)</option>
                    <option value={4} disabled>4 Players (Coming Soon)</option>
                  </select>
                  <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">
                    You'll get a room code to share with your friend
                  </p>
                </div>

                {error && (
                  <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3">
                    <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
                  </div>
                )}

                <div className="flex gap-2">
                  <Button
                    onClick={() => setMode("select")}
                    className="flex-1 py-2 bg-gray-600 hover:bg-gray-700 text-white"
                    disabled={isLoading}
                  >
                    Back
                  </Button>
                  <Button
                    onClick={handleCreateRoom}
                    className="flex-1 py-2 bg-blue-600 hover:bg-blue-700 text-white"
                    disabled={isLoading || maxPlayers > 2}
                  >
                    {isLoading ? (
                      <span className="flex items-center justify-center gap-2">
                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                        Creating...
                      </span>
                    ) : (
                      "Create Room"
                    )}
                  </Button>
                </div>
              </div>
            )}

            {mode === "join" && (
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">
                    Room Code
                  </label>
                  <input
                    type="text"
                    value={roomId}
                    onChange={(e) => setRoomId(e.target.value.toUpperCase())}
                    placeholder="Enter 6-character code"
                    maxLength={6}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 uppercase font-mono text-center text-lg tracking-wider"
                  />
                  <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">
                    Ask your friend for their room code
                  </p>
                </div>

                {error && (
                  <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3">
                    <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
                  </div>
                )}

                <div className="flex gap-2">
                  <Button
                    onClick={() => setMode("select")}
                    className="flex-1 py-2 bg-gray-600 hover:bg-gray-700 text-white"
                    disabled={isLoading}
                  >
                    Back
                  </Button>
                  <Button
                    onClick={handleJoinRoom}
                    className="flex-1 py-2 bg-green-600 hover:bg-green-700 text-white"
                    disabled={isLoading || roomId.length !== 6}
                  >
                    {isLoading ? (
                      <span className="flex items-center justify-center gap-2">
                        <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                        Joining...
                      </span>
                    ) : (
                      "Join Room"
                    )}
                  </Button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </Modal>
  );
};
