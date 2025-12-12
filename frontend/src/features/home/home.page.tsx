import React from 'react';
import { useRouter } from 'next/navigation';
import { motion } from 'framer-motion';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';
import Modal from '@/components/ui/Modal';
import SettingsMenu from '@/components/ui/SettingsMenu';
import AuthButton from '@/features/auth/components/AuthButton';
import { BotDifficultyModal } from '@/components/game/BotDifficultyModal';
import { MultiplayerRoomModal } from '@/features/multiPlayer/components/MultiplayerRoomModal';
import { useHome } from "./hooks/home.hook";
import AboutAndRules from './aboutAndRules/aboutAndRules.component';
import LogoAndTitle from './logoAndTitle/logoAndTitle.component';
import MainMenu from './mainMenu/mainMenu.component';
export default function Home() {
    const router = useRouter();
    const { isAuthenticated,showSettings, setShowSettings, showAbout, setShowAbout,
        showBotDifficulty, setShowBotDifficulty, hasSave, isCreatingGame,
         settings, startNewGame, handleBotDifficultySelect,
        continueSavedGame, handleSettingsChange } = useHome();
    const [showMultiplayerModal, setShowMultiplayerModal] = React.useState(false);


    return (
        <main className="min-h-screen flex items-center justify-center p-4">
            {/* Auth Button in Top Right Corner */}
            <div className="absolute top-4 right-4 flex gap-3">
                {isAuthenticated && (
                    <Button
                        variant="secondary"
                        onClick={() => router.push('/profile')}
                    >
                        {/* <Icon name="user" size={20} className="mr-2" /> */}
                        My Games
                    </Button>
                )}
                <AuthButton />
            </div>

            <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
                className="w-full max-w-2xl"
            >
                {/* Logo and Title */}
                <LogoAndTitle />


                {/* Menu Card */}
               <MainMenu
                    startNewGame={startNewGame}
                    isCreatingGame={isCreatingGame}
                    isAuthenticated={isAuthenticated}
                    hasSave={hasSave}
                    continueSavedGame={continueSavedGame}
                    setShowSettings={setShowSettings}
                    setShowAbout={setShowAbout}
                    onPlayWithFriend={() => setShowMultiplayerModal(true)}
                />

                {/* Quick Settings Display */}
                <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.4 }}
                    className="mt-6 text-center text-sm text-gray-600 dark:text-gray-400"
                >
                    Current: {settings.playerCount} Players · {settings.boardSize}×{settings.boardSize} Board · {settings.theme === 'dark' ? '🌙' : '☀️'} {settings.theme}
                </motion.div>
            </motion.div>

            {/* Settings Modal */}
            <Modal
                isOpen={showSettings}
                onClose={() => setShowSettings(false)}
                title="Settings"
            >
                <SettingsMenu
                    settings={settings}
                    onSettingsChange={handleSettingsChange}
                    onClose={() => setShowSettings(false)}
                    isInGame={false}
                />
            </Modal>

            {/* About Modal */}
            <Modal
                isOpen={showAbout}
                onClose={() => setShowAbout(false)}
                title="About Quoridor"
                size="lg"
            >
                <AboutAndRules/>
            </Modal>

            {/* Bot Difficulty Selection Modal */}
            {showBotDifficulty && (
                <BotDifficultyModal
                    onSelect={handleBotDifficultySelect}
                    onClose={() => setShowBotDifficulty(false)}
                />
            )}

            {/* Multiplayer Room Modal */}
            <MultiplayerRoomModal
                isOpen={showMultiplayerModal}
                onClose={() => setShowMultiplayerModal(false)}
            />
        </main>
    );
}

