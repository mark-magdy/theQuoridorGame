import React from 'react';
import { motion } from 'framer-motion';

interface ToggleProps {
  enabled: boolean;
  onChange: (enabled: boolean) => void;
  label?: string;
  disabled?: boolean;
}

const Toggle: React.FC<ToggleProps> = ({ enabled, onChange, label, disabled = false }) => {
  return (
    <div className="flex items-center gap-3">
      {label && (
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
          {label}
        </span>
      )}
      <button
        type="button"
        onClick={() => !disabled && onChange(!enabled)}
        disabled={disabled}
        className={`
          relative inline-flex h-6 w-11 items-center rounded-full transition-colors
          focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
          disabled:opacity-50 disabled:cursor-not-allowed
          ${enabled ? 'bg-blue-600' : 'bg-gray-300 dark:bg-gray-600'}
        `}
        role="switch"
        aria-checked={enabled}
      >
        <motion.span
          layout
          transition={{ type: 'spring', stiffness: 500, damping: 30 }}
          className={`
            inline-block h-4 w-4 transform rounded-full bg-white transition
            ${enabled ? 'translate-x-6' : 'translate-x-1'}
          `}
        />
      </button>
    </div>
  );
};

export default Toggle;



