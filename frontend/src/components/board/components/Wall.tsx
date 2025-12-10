import React from 'react';
import { motion } from 'framer-motion';
import { Wall as WallType } from '@/types/gameTypes';

interface WallProps {
  wall: WallType;
  cellSize: number;
  isPreview?: boolean;
  isInvalid?: boolean;
}

const Wall: React.FC<WallProps> = ({ wall, cellSize, isPreview = false, isInvalid = false }) => {
  const wallThickness = 8;
  const wallLength = cellSize * 2;

  const { position, isHorizontal } = wall;

  const style: React.CSSProperties = {
    position: 'absolute',
    width: isHorizontal ? wallLength : wallThickness,
    height: isHorizontal ? wallThickness : wallLength,
    top: position.row * cellSize,
    left: position.col * cellSize,
    zIndex: 10,
  };

  return (
    <motion.div
      layout
      initial={isPreview ? { opacity: 0.5, scale: 0.8 } : { scale: 0 }}
      animate={{
        opacity: isPreview ? (isInvalid ? 0.5 : 0.7) : 1,
        scale: 1,
      }}
      exit={{ opacity: 0, scale: 0 }}
      transition={{
        type: 'spring',
        stiffness: 300,
        damping: 25,
      }}
      style={style}
      className={`
        rounded-full shadow-lg
        ${isPreview
          ? isInvalid
            ? 'bg-red-500 border-2 border-red-700'
            : 'bg-blue-400 border-2 border-blue-600'
          : 'bg-gray-800 dark:bg-gray-300 border-2 border-gray-900 dark:border-gray-400'
        }
        ${!isPreview ? 'cursor-default' : 'pointer-events-none'}
      `}
    >
      {/* Inner highlight for depth */}
      {!isPreview && (
        <div
          className="absolute bg-white bg-opacity-20 rounded-full"
          style={{
            width: isHorizontal ? '100%' : '50%',
            height: isHorizontal ? '50%' : '100%',
            top: 0,
            left: 0,
          }}
        />
      )}
    </motion.div>
  );
};

export default Wall;



