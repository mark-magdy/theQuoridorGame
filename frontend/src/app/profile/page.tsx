'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { motion } from 'framer-motion';
import { useAuth } from '@/features/auth/context/AuthContext';
import { gameApi, GameDto } from '@/lib/gameApi';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';

export default function ProfilePage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuth();
  const [activeGames, setActiveGames] = useState<GameDto[]>([]);
  const [finishedGames, setFinishedGames] = useState<GameDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<'active' | 'finished'>('active');

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/');
      return;
    }

    loadGames();
  }, [isAuthenticated, router]);

  const loadGames = async () => {
    try {
      setLoading(true);
      const [active, finished] = await Promise.all([
        gameApi.getMyGames(),
        gameApi.getMyFinishedGames(),
      ]);
      setActiveGames(active);
      setFinishedGames(finished);
    } catch (error) {
      console.error('Error loading games:', error);
    } finally {
      setLoading(false);
    }
  };

  const continueGame = (gameId: string) => {
    router.push(`/game?gameId=${gameId}`);
  };

  const deleteGame = async (gameId: string) => {
    if (!confirm('Are you sure you want to delete this game?')) return;

    try {
      await gameApi.deleteGame(gameId);
      await loadGames();
    } catch (error) {
      console.error('Error deleting game:', error);
      alert('Failed to delete game');
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getGameResult = (game: GameDto) => {
    if (game.gameState.winner === null) return 'In Progress';
    const winnerPlayer = game.gameState.players[game.gameState.winner];
    return winnerPlayer.type === 'Human' ? '🏆 You Won!' : '❌ Bot Won';
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
        <div className="text-2xl font-bold text-gray-900 dark:text-white animate-pulse">
          Loading...
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 p-8">
      {/* Header */}
      <div className="max-w-6xl mx-auto mb-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
              My Games
            </h1>
            <p className="text-gray-600 dark:text-gray-400">
              Welcome back, {user?.username}!
            </p>
          </div>
          <Button variant="secondary" onClick={() => router.push('/')}>
            <Icon name="home" size={20} className="mr-2" />
            Back to Menu
          </Button>
        </div>
      </div>

      {/* Tabs */}
      <div className="max-w-6xl mx-auto mb-6">
        <div className="flex gap-4 border-b border-gray-300 dark:border-gray-700">
          <button
            onClick={() => setTab('active')}
            className={`px-6 py-3 font-semibold transition-colors ${
              tab === 'active'
                ? 'text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400'
                : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
            }`}
          >
            Active Games ({activeGames.length})
          </button>
          <button
            onClick={() => setTab('finished')}
            className={`px-6 py-3 font-semibold transition-colors ${
              tab === 'finished'
                ? 'text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400'
                : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
            }`}
          >
            Finished Games ({finishedGames.length})
          </button>
        </div>
      </div>

      {/* Game List */}
      <div className="max-w-6xl mx-auto">
        {tab === 'active' && (
          <div className="space-y-4">
            {activeGames.length === 0 ? (
              <div className="text-center py-12 bg-white dark:bg-gray-800 rounded-lg">
                <p className="text-gray-600 dark:text-gray-400 mb-4">
                  No active games. Start a new game to begin!
                </p>
                <Button variant="primary" onClick={() => router.push('/')}>
                  Start New Game
                </Button>
              </div>
            ) : (
              activeGames.map((game) => (
                <motion.div
                  key={game.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="bg-white dark:bg-gray-800 rounded-lg p-6 shadow-lg"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                        Game vs Bot ({game.settings.boardSize}×{game.settings.boardSize})
                      </h3>
                      <div className="text-sm text-gray-600 dark:text-gray-400 space-y-1">
                        <p>Started: {formatDate(game.createdAt)}</p>
                        <p>Turn: Player {game.gameState.currentPlayerIndex + 1}</p>
                        <p>Moves: {game.gameState.moveHistory.length}</p>
                      </div>
                    </div>
                    <div className="flex gap-3">
                      <Button
                        variant="primary"
                        onClick={() => continueGame(game.id)}
                      >
                        <Icon name="play" size={20} className="mr-2" />
                        Continue
                      </Button>
                      <Button
                        variant="danger"
                        onClick={() => deleteGame(game.id)}
                      >
                        <Icon name="trash" size={20} />
                      </Button>
                    </div>
                  </div>
                </motion.div>
              ))
            )}
          </div>
        )}

        {tab === 'finished' && (
          <div className="space-y-4">
            {finishedGames.length === 0 ? (
              <div className="text-center py-12 bg-white dark:bg-gray-800 rounded-lg">
                <p className="text-gray-600 dark:text-gray-400">
                  No finished games yet. Complete a game to see it here!
                </p>
              </div>
            ) : (
              finishedGames.map((game) => (
                <motion.div
                  key={game.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="bg-white dark:bg-gray-800 rounded-lg p-6 shadow-lg"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                        Game vs Bot ({game.settings.boardSize}×{game.settings.boardSize})
                      </h3>
                      <div className="text-sm text-gray-600 dark:text-gray-400 space-y-1">
                        <p>Finished: {game.finishedAt ? formatDate(game.finishedAt) : 'N/A'}</p>
                        <p>Total Moves: {game.gameState.moveHistory.length}</p>
                        <p className="text-lg font-semibold mt-2">
                          {getGameResult(game)}
                        </p>
                      </div>
                    </div>
                    <div className="flex gap-3">
                      <Button
                        variant="secondary"
                        onClick={() => continueGame(game.id)}
                      >
                        <Icon name="eye" size={20} className="mr-2" />
                        View
                      </Button>
                      <Button
                        variant="danger"
                        onClick={() => deleteGame(game.id)}
                      >
                        <Icon name="trash" size={20} />
                      </Button>
                    </div>
                  </div>
                </motion.div>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  );
}
