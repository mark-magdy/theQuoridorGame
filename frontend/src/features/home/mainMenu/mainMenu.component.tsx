
import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';

import Tooltip from '@/components/common/Tooltip';



export default function MainMenu({ startNewGame, isCreatingGame, isAuthenticated, hasSave, continueSavedGame, setShowSettings, setShowAbout }
    : { startNewGame: () => void; isCreatingGame: boolean; isAuthenticated: boolean; hasSave: boolean; continueSavedGame: () => void; setShowSettings: React.Dispatch<React.SetStateAction<boolean>>; setShowAbout: React.Dispatch<React.SetStateAction<boolean>>; }) {
    return (<motion.div
        className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 space-y-4"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.2 }}
    >
        <Button
            variant="primary"
            size="lg"
            onClick={startNewGame}
            disabled={isCreatingGame}
            className="w-full text-xl py-4"
        >
            <Icon name="play" size={24} className="inline mr-2" />
            {isCreatingGame ? 'Creating Game...' : 'Start New Game vs Bot'}
        </Button>

        {/* Play with a Friend Button - Requires Authentication */}
        <Tooltip content={!isAuthenticated ? "Sign in to play with friends" : ""}>
            <Button
                variant="success"
                size="lg"
                onClick={() => {
                    // TODO: Implement multiplayer feature in the future
                    alert('Multiplayer feature coming soon!');
                }}
                disabled={!isAuthenticated}
                className="w-full text-xl py-4"
            >
                <Icon name="users" size={24} className="inline mr-2" />
                Play with a Friend
                {!isAuthenticated && (
                    <span className="ml-2 text-xs bg-white/20 px-2 py-1 rounded">
                        🔒 Sign in required
                    </span>
                )}
            </Button>
        </Tooltip>

        {hasSave && (
            <Button
                variant="success"
                size="lg"
                onClick={continueSavedGame}
                className="w-full text-xl py-4"
            >
                <Icon name="load" size={24} className="inline mr-2" />
                Continue Saved Game
            </Button>
        )}

        <Button
            variant="secondary"
            size="lg"
            onClick={() => setShowSettings(true)}
            className="w-full text-xl py-4"
        >
            <Icon name="settings" size={24} className="inline mr-2" />
            Settings
        </Button>

        <Button
            variant="secondary"
            size="lg"
            onClick={() => setShowAbout(true)}
            className="w-full text-xl py-4"
        >
            <Icon name="info" size={24} className="inline mr-2" />
            About & Rules
        </Button>
    </motion.div>);
}