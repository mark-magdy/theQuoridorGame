import React from 'react';
import { useRouter } from 'next/navigation';
import { motion } from 'framer-motion';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';
import Modal from '@/components/ui/Modal';
import SettingsMenu from '@/components/ui/SettingsMenu';
import AuthButton from '@/features/auth/components/AuthButton';
import LoginForm from '@/features/auth/components/LoginForm';
import RegisterForm from '@/features/auth/components/RegisterForm';
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
    const [showAuthModal, setShowAuthModal] = React.useState(false);
    const [authMode, setAuthMode] = React.useState<'login' | 'register'>('login');

    const handleStartNewGame = () => {
        if (!isAuthenticated) {
            setAuthMode('login');
            setShowAuthModal(true);
        } else {
            startNewGame();
        }
    };

    const handlePlayWithFriendClick = () => {
        if (!isAuthenticated) {
            setAuthMode('login');
            setShowAuthModal(true);
        } else {
            setShowMultiplayerModal(true);
        }
    };


    return (
        <main className="min-h-screen flex items-center justify-center p-4 relative overflow-hidden">
            {/* Background Decorations */}
            <div className="absolute inset-0 bg-black/50 pointer-events-none z-0 dark:block hidden"></div>
            <div className="bg-decoration"></div>
            <div className="wall-decoration w-32 h-4 top-1/4 left-1/5 rotate-45 animate-subtle-float" style={{animationDelay: '1s'}}></div>
            <div className="wall-decoration w-4 h-24 top-1/4 left-0 rotate-45 animate-subtle-float" style={{animationDelay: '1s'}}></div>

            <div className="wall-decoration w-4 h-24 top-2/3 right-1/4 -rotate-0 animate-subtle-float" style={{animationDelay: '2s'}}></div>

            <div className="wall-decoration w-4 h-24 top-2/3 left-1/4 rotate-45 animate-subtle-float" style={{animationDelay: '2s'}}></div>
            {/* <div className="wall-decoration w-24 h-4 bottom-1/4 left-1/3 rotate-90 animate-subtle-float"></div> */}
            
            {/* Auth Button in Top Right Corner */}
            <div className="absolute top-4 right-4 flex gap-3 z-20">
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

            <div className="w-full max-w-7xl mx-auto relative z-10 flex flex-col">
                {/* Logo and Title - Centered Top */}
                <motion.div
                    initial={{ opacity: 0, y: -20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5 }}
                    className="w-full flex justify-center"
                >
                    <LogoAndTitle />
                </motion.div>

                {/* Image Left and Menu Right */}
                <div className="flex flex-col lg:flex-row items-center justify-center gap-8 lg:gap-16">
                    {/* Game Simulation - Left Side */}
                    {/* <motion.div
                        initial={{ opacity: 0, x: -50 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ duration: 0.7 }}
                        className="hidden lg:block w-full lg:w-1/2 max-w-xl"
                    >
                        <div className="relative rounded-2xl overflow-hidden">
                            <img src="/quoridorLand.png" alt="Game Simulation" className="w-full h-full object-cover" />
                        </div>
                    </motion.div> */}

                    {/* Main Menu - Right Side */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.5 }}
                        className="w-full lg:w-1/2 max-w-2xl"
                    >
                        {/* Menu Card */}
                        <MainMenu
                            startNewGame={handleStartNewGame}
                            isCreatingGame={isCreatingGame}
                            isAuthenticated={isAuthenticated}
                            hasSave={hasSave}
                            continueSavedGame={continueSavedGame}
                            setShowSettings={setShowSettings}
                            setShowAbout={setShowAbout}
                            onPlayWithFriend={handlePlayWithFriendClick}
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
                </div>
            </div>

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

            {/* Auth Modal for Game Actions */}
            <Modal
                isOpen={showAuthModal}
                onClose={() => setShowAuthModal(false)}
                title={authMode === 'login' ? 'Sign In' : 'Create Account'}
            >
                {authMode === 'login' ? (
                    <LoginForm
                        onSuccess={() => setShowAuthModal(false)}
                        onSwitchToRegister={() => setAuthMode('register')}
                    />
                ) : (
                    <RegisterForm
                        onSuccess={() => setShowAuthModal(false)}
                        onSwitchToLogin={() => setAuthMode('login')}
                    />
                )}
            </Modal>
        </main>
    );
}

