'use client';

import React from 'react';
import { motion } from 'framer-motion';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';
import { BotDifficulty } from '@/lib/utils';

interface BotDifficultyModalProps {
  onSelect: (difficulty: BotDifficulty) => void;
  onClose: () => void;
}

const difficultyOptions = [
  {
    level: BotDifficulty.Easy,
    title: 'Easy',
    description: 'Perfect for beginners. The bot makes simple moves.',
    icon: '🙂',
    color: 'bg-green-500 hover:bg-green-600',
  },
  {
    level: BotDifficulty.Medium,
    title: 'Medium',
    description: 'A balanced challenge. The bot thinks ahead.',
    icon: '🤔',
    color: 'bg-yellow-500 hover:bg-yellow-600',
  },
  {
    level: BotDifficulty.Hard,
    title: 'Hard',
    description: 'For experienced players. The bot is strategic.',
    icon: '🤯',
    color: 'bg-red-500 hover:bg-red-600',
  },
];

export const BotDifficultyModal: React.FC<BotDifficultyModalProps> = ({
  onSelect,
  onClose,
}) => {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.9 }}
        className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 max-w-2xl w-full"
      >
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-3xl font-bold text-gray-900 dark:text-white">
            Choose Bot Difficulty
          </h2>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
          >
            <Icon name="close" size={24} />
          </button>
        </div>

        {/* Difficulty Options */}
        <div className="space-y-4">
          {difficultyOptions.map((option) => (
            <motion.button
              key={option.level}
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              onClick={() => onSelect(option.level)}
              className="w-full text-left p-6 rounded-xl border-2 border-gray-200 dark:border-gray-700 hover:border-blue-500 dark:hover:border-blue-400 transition-all"
            >
              <div className="flex items-start gap-4">
                <div className="text-4xl">{option.icon}</div>
                <div className="flex-1">
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                    {option.title}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-400">
                    {option.description}
                  </p>
                </div>
                <Icon name="chevron-right" size={24} className="text-gray-400" />
              </div>
            </motion.button>
          ))}
        </div>

        {/* Cancel Button */}
        <div className="mt-6">
          <Button
            variant="secondary"
            size="lg"
            onClick={onClose}
            className="w-full"
          >
            Cancel
          </Button>
        </div>
      </motion.div>
    </div>
  );
};
