"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useGameHub } from "@/features/multiPlayer/hooks/useGameHub";
import { RoomModal } from "@/features/multiPlayer/components/RoomModal";
import Button from "@/components/common/Button";

export default function MultiplayerLobbyPage() {
  const router = useRouter();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const { isConnected, error, createRoom, joinRoom } = useGameHub();

  useEffect(() => {
    // Check if we're in the browser
    if (typeof window === "undefined") return;
    
    // Check if user is authenticated
    // const token = localStorage.getItem("token");
    // if (!token) {
    //   router.push("/");
    //   return;
    // }
    setIsAuthenticated(true);
    
    // Open modal automatically
    setIsModalOpen(true);
  }, [router]);

  const handleCreateRoom = async (maxPlayers: number) => {
    try {
      const room = await createRoom(maxPlayers);
      // Store room data in sessionStorage
      sessionStorage.setItem("currentRoom", JSON.stringify(room));
      // Navigate to game page
      router.push("/MultiPlayerGame");
    } catch (err) {
      console.error("Failed to create room:", err);
      throw err;
    }
  };

  const handleJoinRoom = async (roomId: string) => {
    try {
      const room = await joinRoom(roomId);
      // Store room data in sessionStorage
      sessionStorage.setItem("currentRoom", JSON.stringify(room));
      // Navigate to game page
      router.push("/MultiPlayerGame");
    } catch (err) {
      console.error("Failed to join room:", err);
      throw err;
    }
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    router.push("/");
  };

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-900 via-purple-900 to-pink-900">
        <p className="text-white text-xl">Checking authentication...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-900 via-purple-900 to-pink-900 p-4">
      <div className="max-w-2xl w-full bg-white/10 backdrop-blur-md rounded-lg p-8 text-center">
        <h1 className="text-4xl font-bold text-white mb-4">Multiplayer Lobby</h1>
        
        {!isConnected && (
          <div className="mb-6">
            <p className="text-white/80 mb-2">Connecting to game server...</p>
            {error && <p className="text-red-400">{error}</p>}
          </div>
        )}

        {isConnected && (
          <div className="space-y-4">
            <p className="text-white/80 mb-6">Create a new room or join an existing one to play with friends!</p>
            
            <div className="flex gap-4 justify-center">
              <Button
                onClick={() => setIsModalOpen(true)}
                className="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-lg"
              >
                Create / Join Room
              </Button>
              
              <Button
                onClick={() => router.push("/")}
                className="px-8 py-3 bg-gray-600 hover:bg-gray-700 text-lg"
              >
                Back to Menu
              </Button>
            </div>
          </div>
        )}
      </div>

      <RoomModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        onCreateRoom={handleCreateRoom}
        onJoinRoom={handleJoinRoom}
      />
    </div>
  );
}
