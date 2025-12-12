"use client";

import { useState } from "react";
import Modal from "@/components/ui/Modal";
import Button from "@/components/common/Button";

interface RoomModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreateRoom: (maxPlayers: number) => Promise<void>;
  onJoinRoom: (roomId: string) => Promise<void>;
}

export const RoomModal = ({ isOpen, onClose, onCreateRoom, onJoinRoom }: RoomModalProps) => {
  const [mode, setMode] = useState<"select" | "create" | "join">("select");
  const [maxPlayers, setMaxPlayers] = useState(2);
  const [roomId, setRoomId] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCreateRoom = async () => {
    setIsLoading(true);
    setError(null);
    try {
      await onCreateRoom(maxPlayers);
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

    setIsLoading(true);
    setError(null);
    try {
      await onJoinRoom(roomId.toUpperCase());
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
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Multiplayer Game">
      <div className="space-y-4">
        {mode === "select" && (
          <div className="space-y-3">
            <Button
              onClick={() => setMode("create")}
              className="w-full py-3 bg-blue-600 hover:bg-blue-700"
            >
              Create New Room
            </Button>
            <Button
              onClick={() => setMode("join")}
              className="w-full py-3 bg-green-600 hover:bg-green-700"
            >
              Join Existing Room
            </Button>
          </div>
        )}

        {mode === "create" && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">Number of Players</label>
              <select
                value={maxPlayers}
                onChange={(e) => setMaxPlayers(Number(e.target.value))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md bg-white text-gray-900"
              >
                <option value={2}>2 Players</option>
                <option value={3}>3 Players (Coming Soon)</option>
                <option value={4}>4 Players (Coming Soon)</option>
              </select>
            </div>

            {error && <p className="text-red-500 text-sm">{error}</p>}

            <div className="flex gap-2">
              <Button
                onClick={() => setMode("select")}
                className="flex-1 py-2 bg-gray-600 hover:bg-gray-700"
                disabled={isLoading}
              >
                Back
              </Button>
              <Button
                onClick={handleCreateRoom}
                className="flex-1 py-2 bg-blue-600 hover:bg-blue-700"
                disabled={isLoading || maxPlayers > 2}
              >
                {isLoading ? "Creating..." : "Create Room"}
              </Button>
            </div>
          </div>
        )}

        {mode === "join" && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">Room ID</label>
              <input
                type="text"
                value={roomId}
                onChange={(e) => setRoomId(e.target.value.toUpperCase())}
                placeholder="Enter 6-character room code"
                maxLength={6}
                className="w-full px-3 py-2 border border-gray-300 rounded-md bg-white text-gray-900 uppercase"
              />
            </div>

            {error && <p className="text-red-500 text-sm">{error}</p>}

            <div className="flex gap-2">
              <Button
                onClick={() => setMode("select")}
                className="flex-1 py-2 bg-gray-600 hover:bg-gray-700"
                disabled={isLoading}
              >
                Back
              </Button>
              <Button
                onClick={handleJoinRoom}
                className="flex-1 py-2 bg-green-600 hover:bg-green-700"
                disabled={isLoading || roomId.length !== 6}
              >
                {isLoading ? "Joining..." : "Join Room"}
              </Button>
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
};
