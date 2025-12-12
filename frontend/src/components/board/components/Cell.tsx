import React from 'react';
import { motion } from 'framer-motion';
import { Position } from '@/types/gameTypes';

interface CellProps {
  position: Position;
  cellSize: number;
  isValidMove: boolean;
  isSelected: boolean;
  isCurrentPlayer:boolean; 
  onClick: () => void;
  onMouseEnter?: () => void;
  onMouseLeave?: () => void;
}

const Cell: React.FC<CellProps> = ({
  position,
  cellSize,
  isValidMove,
  isSelected,
  isCurrentPlayer,
  onClick,
  onMouseEnter,
  onMouseLeave,
}) => {
  return (
    <motion.div
      className={`
        absolute border border-gray-400 dark:border-gray-600
        cursor-pointer transition-all duration-150
        ${isSelected ? 'bg-blue-100 dark:bg-blue-900 ring-2 ring-blue-500' : 'bg-transparent'}
        ${isValidMove ? 'hover:bg-green-100 dark:hover:bg-green-900 hover:scale-105' : 'hover:bg-gray-50 dark:hover:bg-gray-700'}
      `}
      style={{
        width: cellSize,
        height: cellSize,
        top: position.row * cellSize,
        left: position.col * cellSize,
      }}
      onClick={onClick}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
      whileHover={{ scale: isValidMove ? 1.05 : 1.02 }}
      whileTap={{ scale: 0.95 }}
      transition={{ duration: 0.1 }}
    >
      {/* Valid move indicator */}
      {isValidMove && isCurrentPlayer && (
        <motion.div
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          exit={{ scale: 0 }}
          className="absolute inset-0 flex items-center justify-center"
        >
          <motion.div
            animate={{
              scale: [1, 1.3, 1],
            }}
            transition={{
              duration: 1.5,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
            className="w-4 h-4 rounded-full bg-green-500 shadow-lg"
          />
        </motion.div>
      )}

      {/* Hover effect for valid moves */}
      {isValidMove && (
        <div className="absolute inset-0 border-2 border-green-400 rounded opacity-50" />
      )}
    </motion.div>
  );
};

export default Cell;
