'use client';

import React, { useState, useMemo, use } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { GameState, Position, Wall as WallType } from '@/types/gameTypes';
import Cell from './components/Cell';
import Pawn from './components/Pawn';
import Wall from './components/Wall';
import SlideToggle from '@/components/common/SlideToggle';
import { useBoard } from './hooks/useBoard.hook';

interface BoardProps {
  gameState: GameState;
  onPawnMove: (position: Position) => void;
  onWallPlace: (wall: WallType) => Promise<boolean>;
  cellSize?: number;
}

const Board: React.FC<BoardProps> = ({
  gameState,
  onPawnMove,
  onWallPlace,
  cellSize = 60,
}) => {
  const [isPlacingWall, setIsPlacingWall] = useState(false);
  const [placementError, setPlacementError] = useState<string>('');

  const boardSize = gameState.boardSize;
  const totalSize = boardSize * cellSize;
  const currentPlayer = gameState.players[gameState.currentPlayerIndex];
  const me = localStorage.getItem('userId');
  const isCurrentPlayer = currentPlayer.userId === me;
  // If game is not loaded yet, show loading state
  if (!currentPlayer) {
    return (
      <div className="flex items-center justify-center gap-2">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
        <span className="text-gray-600 dark:text-gray-400">Loading game...</span>
      </div>
    );
  }

  const {
    cells,
    hoveredWallPosition,
    isValidMove,
    isSelected,
    handleCellClick: onCellClick,
    handleWallHover,
    handleWallClick,
    isWallValid,
    setHoveredWallPosition,
  } = useBoard({
    gameState,
    onPawnMove,
    onWallPlace,
    isPlacingWall,
    setIsPlacingWall,
    setPlacementError,
  });

  return (
    <div className="flex flex-col items-center gap-4 w-full">
      {/* Mode Toggle - Slider */}
      <div className="w-full max-w-md px-4">
        <SlideToggle
          value={isPlacingWall}
          onChange={(value) => {
            if (currentPlayer && currentPlayer.wallsRemaining > 0 && gameState.gameStatus === 'Playing') {
              setIsPlacingWall(value);
              setPlacementError('');
            }
          }}
          leftLabel="Move Pawn"
          rightLabel="Place Wall"
          leftIcon="🚶"
          rightIcon="🧱"
          disabled={!currentPlayer || !isCurrentPlayer || currentPlayer.wallsRemaining <= 0 || gameState.gameStatus !== 'Playing'}
        />
      </div>

     

      {/* Board container */}
      <motion.div
        initial={{ scale: 0.9, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ type: 'spring', duration: 0.5 }}
        className="relative bg-gradient-to-br from-amber-200 to-amber-300 dark:from-amber-900 dark:to-amber-800 rounded-xl shadow-2xl p-2"
        style={{
          width: totalSize + 16+8,
          height: totalSize + 16+8,
        }}
      >
        <div
          className="relative bg-amber-50 dark:bg-gray-800 border-4 border-amber-800 dark:border-amber-200 rounded-lg shadow-inner"
          style={{
            width: totalSize+8,
            height: totalSize+8,
          }}
        >
          {/* Cells */}
          {cells.map((position) => (
            <Cell
              isCurrentPlayer={isCurrentPlayer}
              key={`${position.row}-${position.col}`}
              position={position}
              cellSize={cellSize}
              isValidMove={isValidMove(position)}
              isSelected={isSelected(position)}
              onClick={() => !isPlacingWall && onCellClick(position)}
            />
          ))}

          {/* Wall slots (horizontal and vertical) */}
          {isPlacingWall && gameState.gameStatus === 'Playing' && currentPlayer.wallsRemaining > 0 && (
            <>
              {/* Horizontal wall slots */}
              {Array.from({ length: boardSize }).map((_, row) =>
                Array.from({ length: boardSize - 1 }).map((_, col) => {
                  const isHovered = hoveredWallPosition?.position.row === row &&
                                   hoveredWallPosition?.position.col === col &&
                                   hoveredWallPosition?.isHorizontal;
                  return (
                    <div
                      key={`h-slot-${row}-${col}`}
                      className={`
                        absolute cursor-pointer z-5 transition-all
                        ${isHovered ? 'bg-blue-400 bg-opacity-50 scale-110' : 'hover:bg-blue-200 hover:bg-opacity-30'}
                      `}
                      style={{
                        width: cellSize * 2,
                        height: 12,
                        top: row * cellSize - 6,
                        left: col * cellSize,
                      }}
                      onMouseEnter={() => handleWallHover(row, col, true)}
                      onMouseLeave={() => setHoveredWallPosition(null)}
                      onClick={() => handleWallClick(row, col, true)}
                    />
                  );
                })
              )}

              {/* Vertical wall slots */}
              {Array.from({ length: boardSize - 1 }).map((_, row) =>
                Array.from({ length: boardSize }).map((_, col) => {
                  const isHovered =col!==0 && hoveredWallPosition?.position.row === row &&
                                   hoveredWallPosition?.position.col === col &&
                                   !hoveredWallPosition?.isHorizontal;

                  return (
                    <div
                      key={`v-slot-${row}-${col}`}
                      className={`
                        absolute cursor-pointer z-5 transition-all
                        ${isHovered ? 'bg-blue-400 bg-opacity-50 scale-110' : 'hover:bg-blue-200 hover:bg-opacity-30'}
                      `}
                      style={{
                        width: 12,
                        height: cellSize * 2,
                        top: row * cellSize,
                        left: col * cellSize - 6,
                      }}
                      onMouseEnter={() => handleWallHover(row, col, false)}
                      onMouseLeave={() => setHoveredWallPosition(null)}
                      onClick={() => handleWallClick(row, col, false)}
                    />
                  );
                })
              )}
            </>
          )}

          {/* Walls */}
          <AnimatePresence>
            {gameState.walls.map((wall, index) => (
              <Wall
                key={`wall-${index}-${wall.position.row}-${wall.position.col}-${wall.isHorizontal}`}
                wall={wall}
                cellSize={cellSize}
              />
            ))}
          </AnimatePresence>

          {/* Wall preview */}
          {hoveredWallPosition && isPlacingWall && (
            <Wall
              wall={{
                position: hoveredWallPosition.position,
                isHorizontal: hoveredWallPosition.isHorizontal,
              }}
              cellSize={cellSize}
              isPreview
              isInvalid={!isWallValid({
                position: hoveredWallPosition.position,
                isHorizontal: hoveredWallPosition.isHorizontal,
              })}
            />
          )}

          {/* Pawns */}
          <AnimatePresence>
            {gameState.players.map((player) => (
              <div
                key={`pawn-container-${player.id}`}
                className={`absolute ${player.id === currentPlayer.id && !isPlacingWall ? 'cursor-pointer' : 'pointer-events-none'}`}
                style={{
                  width: cellSize*0.35,
                  height: cellSize*0.35,
                  top: player.position.row * cellSize,
                  left: player.position.col * cellSize,
                  zIndex: 30,
                }}
                onClick={() => {
                  if (player.id === currentPlayer.id && !isPlacingWall) {
                    onCellClick(player.position);
                  }
                }}
              >
                <Pawn
                  player={player}
                  isCurrentPlayer={player.id === currentPlayer.id}
                  cellSize={cellSize}
                />
              </div>
            ))}
          </AnimatePresence>
        </div>
      </motion.div>

      {/* Legend */}
      <div className="text-xs text-gray-600 dark:text-gray-400 text-center space-y-1">
        <div className="flex items-center justify-center gap-4 flex-wrap">
          <span className="flex items-center gap-1">
            <span className="w-3 h-3 rounded-full bg-green-500"></span> Valid move
          </span>
          <span className="flex items-center gap-1">
            <span className="w-3 h-3 rounded-full bg-blue-500 animate-pulse"></span> Current player
          </span>
          <span className="flex items-center gap-1">
            <span className="w-4 h-2 rounded bg-gray-800"></span> Wall
          </span>
        </div>
      </div>
       {/* Error Message */}
      <AnimatePresence>
        {placementError && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className="bg-red-100 dark:bg-red-900 border-2 border-red-400 text-red-800 dark:text-red-200 px-4 py-2 rounded-lg font-semibold"
          >
            {placementError}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default Board;
