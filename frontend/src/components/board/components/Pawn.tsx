import React from 'react';
import { motion } from 'framer-motion';
import { Player } from '@/types/gameTypes';
import { PLAYER_COLOR_CLASSES, PLAYER_BORDER_CLASSES } from '@/lib/constants';

interface PawnProps {
  player: Player;
  isCurrentPlayer: boolean;
  cellSize: number;
}

const Pawn: React.FC<PawnProps> = ({ player, isCurrentPlayer, cellSize }) => {
  const pawnSize = cellSize * 0.7;
  
  // Determine text color based on player color for better contrast
  const getTextColor = (color: string) => {
    // Yellow needs dark text for better visibility
    if (color === 'player4') return 'text-gray-900';
    return 'text-white';
  };

  return (
    <motion.div
      layout
      layoutId={`pawn-${player.id}`}
      initial={{ scale: 0, opacity: 0 }}
      animate={{
        scale: 1,
        opacity: 1,
      }}
      exit={{ scale: 0, opacity: 0 }}
      whileHover={isCurrentPlayer ? { scale: 1.15 } : {}}
      transition={{
        layout: {
          type: 'spring',
          stiffness: 300,
          damping: 30,
        },
        default: {
          duration: 0.3,
        },
      }}
      className={`
        absolute rounded-full
        ${PLAYER_COLOR_CLASSES[player.color]}
        border-4 ${PLAYER_BORDER_CLASSES[player.color]}
        z-20 shadow-lg
        ${isCurrentPlayer ? 'ring-4 ring-blue-400 ring-opacity-70 cursor-pointer' : ''}
      `}
      style={{
        width: pawnSize,
        height: pawnSize,
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        boxShadow: isCurrentPlayer
          ? '0 0 30px rgba(59, 130, 246, 0.8), 0 4px 15px rgba(0, 0, 0, 0.3)'
          : '0 4px 15px rgba(0, 0, 0, 0.3)',
      }}
    >
      {/* Inner circle for depth */}
      <div className="absolute inset-2 rounded-full bg-white bg-opacity-30 shadow-inner" />
      
      {/* Player number indicator */}
      <div className="absolute inset-0 flex items-center justify-center">
        <span
          className={`
            font-bold text-lg drop-shadow-[0_2px_4px_rgba(0,0,0,0.8)]
            text-gray-700
            dark:text-white
          `}
        >
          {player.id + 1}
        </span>
      </div>

      {/* Pulse animation for current player */}
      {isCurrentPlayer && (
        <>
          <motion.div
            className={`absolute inset-0 rounded-full ${PLAYER_COLOR_CLASSES[player.color]} opacity-40`}
            animate={{
              scale: [1, 1.4, 1],
              opacity: [0.4, 0, 0.4],
            }}
            transition={{
              duration: 2,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
          <motion.div
            className="absolute -inset-1 rounded-full border-2 border-blue-400"
            animate={{
              scale: [1, 1.15, 1],
              opacity: [1, 0.5, 1],
            }}
            transition={{
              duration: 1.5,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        </>
      )}

      {/* Click hint for current player */}
      {/* {isCurrentPlayer && (
        <motion.div
          className="absolute -top-8 left-1/2 transform -translate-x-1/2 bg-blue-600 text-white text-xs px-2 py-1 rounded whitespace-nowrap shadow-lg"
          initial={{ opacity: 0, y: 5 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
        >
          Click me!
        </motion.div>
      )} */}
    </motion.div>
  );
};

export default Pawn;
