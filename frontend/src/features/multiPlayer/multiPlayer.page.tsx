"use client";

import Board from "@/components/board/Board";
import Button from "@/components/common/Button";
import { useMultiplayerGame } from "./hooks/useMultiplayerGame";

export default function MultiPlayerGamePage() {
  const {
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
  } = useMultiplayerGame();

  if (!room) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p>Loading room...</p>
      </div>
    );
  }
  // console.log("Room ", room);
  if (!isConnected) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-xl mb-2">Connecting to game server...</p>
          {error && <p className="text-red-500">{error}</p>}
        </div>
      </div>
    );
  }
  // console.log(">> game state: ", gameState);
  return (
    <div className="relative min-h-screen bg-gradient-to-br from-gray-400 to-gray-800 p-4 overflow-hidden">
      <div className="absolute inset-0 bg-black/50 pointer-events-none z-0"></div>
      <div className="bg-decoration"></div>
      <div className="wall-decoration w-32 h-4 top-1/4 left-[20%] rotate-45 animate-subtle-float" style={{ animationDelay: '1s' }}></div>
      <div className="wall-decoration w-4 h-24 top-1/4 left-0 rotate-45 animate-subtle-float" style={{ animationDelay: '1s' }}></div>
      <div className="wall-decoration w-4 h-24 top-2/3 right-[25%] animate-subtle-float" style={{ animationDelay: '2s' }}></div>
      <div className="wall-decoration w-4 h-24 top-2/3 left-[25%] rotate-45 animate-subtle-float" style={{ animationDelay: '2s' }}></div>
      <div className="max-w-7xl mx-auto relative z-10">
        {/* Header */}
        <div className="bg-white/10 backdrop-blur-md rounded-lg p-4 mb-4">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-2xl font-bold text-white mb-2">Multiplayer Game</h1>
              <div className="flex items-center gap-2">
                <span className="text-white/80">Room ID:</span>
                <span className="text-xl font-mono text-white bg-black/30 px-3 py-1 rounded">
                  {room.roomId}
                </span>
                <Button
                  onClick={handleCopyRoomId}
                  className="px-3 py-1 text-sm bg-blue-600 hover:bg-blue-700"
                >
                  {copySuccess ? "Copied!" : "Copy"}
                </Button>
              </div>
            </div>
            <Button
              onClick={handleLeaveRoom}
              className="px-4 py-2 bg-red-600 hover:bg-red-700"
            >
              Leave Room
            </Button>
          </div>
        </div>

        {/* Waiting Room */}
        {!isGameStarted && (
          <div className="bg-white/10 backdrop-blur-md rounded-lg p-6 mb-4">
            <h2 className="text-xl font-bold text-white mb-4">
              Waiting for players... ({room.currentPlayers}/{room.maxPlayers})
            </h2>

            <div className="space-y-2 mb-6">
              {room.players.map((player) => (
                <div
                  key={player.userId}
                  className="bg-white/5 rounded px-4 py-2 flex justify-between items-center"
                >
                  <span className="text-white">
                    {player.username}
                    {player.isHost && <span className="ml-2 text-yellow-400">(Host)</span>}
                  </span>
                  <span className={player.isReady ? "text-green-400" : "text-gray-400"}>
                    {player.isReady ? "Ready" : "Not Ready"}
                  </span>
                </div>
              ))}
            </div>

            {canStartGame && (
              <Button
                onClick={handleStartGame}
                className="w-full py-3 bg-green-600 hover:bg-green-700 text-lg font-bold"
              >
                Start Game
              </Button>
            )}

            {!isHost && (
              <p className="text-white/60 text-center">Waiting for host to start the game...</p>
            )}
          </div>
        )}

        {/* Game Board */}
        {isGameStarted && gameState && (
          <div className="bg-white/10 backdrop-blur-md rounded-lg p-6">
            {/* Game Info */}
            <div className="mb-4 flex justify-between items-center">
              <div className="text-white">
                {gameState.gameStatus === 'Finished' && gameState.winner !== null ? (
                  <div className="space-y-2">
                    <div className="text-2xl font-bold">
                      {gameState.players.find(p => p.id === gameState.winner) && (
                        <span className="text-green-400">
                          🏆 {gameState.players.find(p => p.id === gameState.winner)?.name || `Player ${gameState.winner + 1}`} Wins!
                        </span>
                      )}
                    </div>
                    <div className="text-sm text-white/70">
                      {currentUserPlayer && currentUserPlayer.id === gameState.winner ? (
                        <span className="text-green-300">Congratulations! You won! 🎉</span>
                      ) : currentUserPlayer ? (
                        <span className="text-red-300">You lost. Better luck next time! 💪</span>
                      ) : (
                        <span>Game Over</span>
                      )}
                    </div>
                  </div>
                ) : (
                  <>
                    <span className="font-bold">Current Turn: </span>
                    <span className="text-lg">
                      {gameState.players[gameState.currentPlayerIndex]?.name || `Player ${gameState.currentPlayerIndex + 1}`}
                    </span>
                  </>
                )}
              </div>
              {gameState.gameStatus !== 'Finished' && (
                <>
                  {isSpectator && (
                    <div className="bg-yellow-500/20 text-yellow-300 px-4 py-2 rounded">
                      👁️ Spectator Mode
                    </div>
                  )}
                  {!isCurrentPlayerTurn && !isSpectator && (
                    <div className="bg-blue-500/20 text-blue-300 px-4 py-2 rounded">
                      Waiting for opponent...
                    </div>
                  )}
                  {isCurrentPlayerTurn && (
                    <div className="bg-green-500/20 text-green-300 px-4 py-2 rounded animate-pulse">
                      Your Turn!
                    </div>
                  )}
                </>
              )}
            </div>

            {/* Players and Walls Info */}
            <div className="mb-4 grid grid-cols-2 gap-3">
              {gameState.players.map((player) => (
                <div
                  key={player.id}
                  className={`bg-white/5 rounded-lg p-3 border-2 ${gameState.currentPlayerIndex === player.id
                      ? 'border-green-400'
                      : 'border-transparent'
                    } ${currentUserPlayer?.id === player.id ? 'bg-white/10' : ''}`}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <div
                        className={`w-4 h-4 rounded-full ${player.color === 'player1'
                            ? 'bg-blue-500'
                            : player.color === 'player2'
                              ? 'bg-red-500'
                              : player.color === 'player3'
                                ? 'bg-green-500'
                                : 'bg-yellow-500'
                          }`}
                      />
                      <span className="text-white font-medium">
                        {player.name}
                        {currentUserPlayer?.id === player.id && (
                          <span className="ml-1 text-xs text-green-400">(You)</span>
                        )}
                      </span>
                    </div>
                    <div className="flex items-center gap-1 text-white">
                      <span className="text-lg">🧱</span>
                      <span className="font-bold text-lg">{player.wallsRemaining}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
            <Board
              gameState={gameState}
              onPawnMove={isCurrentPlayerTurn ? handlePawnMove : () => { }}
              onWallPlace={isCurrentPlayerTurn ? handleWallPlace : async () => false}
            />
          </div>
        )}
      </div>
    </div>
  );
}
