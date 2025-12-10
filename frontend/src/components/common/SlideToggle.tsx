import React from 'react';
import { motion } from 'framer-motion';

interface SlideToggleProps {
  value: boolean;
  onChange: (value: boolean) => void;
  leftLabel: string;
  rightLabel: string;
  leftIcon?: string;
  rightIcon?: string;
  disabled?: boolean;
}

const SlideToggle: React.FC<SlideToggleProps> = ({
  value,
  onChange,
  leftLabel,
  rightLabel,
  leftIcon,
  rightIcon,
  disabled = false,
}) => {
  return (
    <div className="w-full  max-w-xs mx-auto">
      <button
        type="button"
        onClick={() => !disabled && onChange(!value)}
        disabled={disabled}
        aria-pressed={value}
        className={`
          group relative w-full h-12 px-4 flex items-center rounded-full outline-none focus:ring-2 focus:ring-blue-400 transition-all
          ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
          bg-gray-200 dark:bg-gray-700
        `}
      >
        {/* Track */}
        <div className="absolute inset-0 rounded-full bg-gray-300 dark:bg-gray-700 transition-all" />

        {/* Color highlight under knob */}
        <motion.div
          className="absolute top-1 left-1 h-10 w-[calc(50%-0.25rem)] rounded-full"
          style={{
            background: value
              ? 'linear-gradient(90deg, #f97316 30%, #fbbf24 100%)'
              : 'linear-gradient(90deg, #22c55e 30%, #10b981 100%)'
          }}
          initial={false}
          animate={{
            x: value ? '100%' : '0%',
            opacity: 0.75,
          }}
          transition={{ type: 'spring', stiffness: 400, damping: 32 }}
        />

        {/* Sliding knob */}
        <motion.div
          className={`
            absolute top-1 left-1 h-10 w-[calc(50%-0.25rem)] rounded-full shadow-lg z-10
            bg-white dark:bg-gray-200
            flex items-center justify-center
            border-2 border-gray-300 dark:border-gray-400
            transition-colors
          `}
          initial={false}
          animate={{
            x: value ? '100%' : '0%',
          }}
          transition={{ type: 'spring', stiffness: 400, damping: 32 }}
        >
          {/* Optionally, put the selected icon inside the knob for more clarity */}
          {/* {value
            ? (rightIcon && <span className="text-xl text-orange-500">{rightIcon}</span>)
            : (leftIcon && <span className="text-xl text-green-600">{leftIcon}</span>)
          } */}
        </motion.div>

        {/* Labels */}
        <div className="w-full px-2 flex items-center justify-between z-20 pointer-events-none relative">
          {/* Left label */}
          <div
            className={`
              flex items-center gap-2 transition-all duration-200
              ${!value
                ? 'text-green-700 dark:text-green-400 font-bold'
                : 'text-gray-600 dark:text-gray-400 font-normal'
              }
              ${!value ? 'scale-105' : 'scale-95'}
            `}
          >
            {leftIcon && <span className="text-lg">{leftIcon}</span>}
            <span className="text-sm">{leftLabel}</span>
          </div>
          {/* Right label */}
          <div
            className={`
              flex items-center gap-2 transition-all duration-200
              ${value
                ? 'text-orange-600 dark:text-orange-300 font-bold'
                : 'text-gray-600 dark:text-gray-400 font-normal'
              }
              ${value ? 'scale-105' : 'scale-95'}
            `}
          >
            {rightIcon && <span className="text-lg">{rightIcon}</span>}
            <span className="text-sm">{rightLabel}</span>
          </div>
        </div>
      </button>
      {/* Helper text */}
      <div className="text-center mt-2 text-xs text-gray-600 dark:text-gray-400 px-2">
        {value ? (
          <span>Click between cells to place walls</span>
        ) : (
          <span>Click your pawn, then a valid move</span>
        )}
      </div>
    </div>
  );
};

export default SlideToggle;
