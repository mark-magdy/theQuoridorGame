'use client';

import React from 'react';
import { useRouter } from 'next/navigation';
import Board from '../../components/board/Board';
import { useGameState } from './hooks/useGameBotState.hook';
import { GameStatusBar } from './components/gameStatusBar.component';
import { Instructions } from './components/instructions.component';


export default function GameVsBot() {
  const router = useRouter();
  const {
    gameState,
    placeWall,
    movePawn,
    restart,
    isLoading,
    error,
    isPlayerTurn,
    gameId,
  } = useGameState();

  // Fix SSR error: use state for cellSize and set in useEffect
  const [cellSize, setCellSize] = React.useState(55);
  React.useEffect(() => {
    if (typeof window !== 'undefined') {
      if (window.innerWidth < 640) setCellSize(35);
      else if (window.innerWidth < 1024) setCellSize(45);
      else setCellSize(55);
    }
  }, []);

  const handleBackToMenu = () => {
    router.push('/');
  };

  // Loading state
  if (!gameId && isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
        <div className="text-center">
          <div className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            Loading game...
          </div>
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
        </div>
      </div>
    );
  }

  // Error state
  if (error && !gameId) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
        <div className="text-center bg-white dark:bg-gray-800 p-8 rounded-lg shadow-xl">
          <div className="text-2xl font-bold text-red-600 dark:text-red-400 mb-4">
            Error Loading Game
          </div>
          <div className="text-gray-700 dark:text-gray-300 mb-6">{error}</div>
          <button
            onClick={handleBackToMenu}
            className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Back to Menu
          </button>
        </div>
      </div>
    );
  }

  const currentPlayer = gameState.players[gameState.currentPlayerIndex];
  const isGameFinished = gameState.gameStatus === 'Finished';
  const winner = gameState.winner !== null ? gameState.players.find(p => p.id === gameState.winner) : null;

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 p-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-4 flex justify-between items-center">
          <button
            onClick={handleBackToMenu}
            className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
          >
            ← Back to Menu
          </button>

          <div className="text-center">
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
              Quoridor vs Bot
            </h1>
          </div>

          <button
            onClick={restart}
            disabled={isLoading}
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            New Game
          </button>
        </div>

        {/* Game Status Bar */}
        <GameStatusBar
          isGameFinished={isGameFinished}
          winner={winner}
          isPlayerTurn={isPlayerTurn}
          currentPlayer={currentPlayer}
          gameState={gameState}
        />


        {/* Error display */}
        {error && (
          <div className="mt-2 text-sm text-red-600 dark:text-red-400 text-center">
            {error}
          </div>
        )}
      </div>

      {/* Game Board */}
      <div className="flex-1 flex flex-col items-center justify-center gap-2 overflow-hidden min-h-0">
        <div className="flex items-center justify-center max-h-full">
          <Board
            gameState={gameState}
            onWallPlace={placeWall}
            onPawnMove={movePawn}
            cellSize={cellSize}
          />
        </div>
      </div>

      {/* Loading indicator */}
      {isLoading && (
        <div className="mt-2 flex items-center justify-center gap-2">
          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-indigo-600"></div>
          <span className="text-sm text-gray-600 dark:text-gray-400">
            {isPlayerTurn ? 'Processing move...' : 'Bot is making a move...'}
          </span>
        </div>
      )}

      {/* Instructions */}
      <Instructions />
    </div>
  );
}
