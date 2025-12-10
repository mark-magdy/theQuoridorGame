import { GameState, Player } from "@/types/gameTypes";

export function GameStatusBar({ isGameFinished, winner, isPlayerTurn, currentPlayer, gameState }:
     { isGameFinished: boolean; winner?: Player|null; isPlayerTurn: boolean; currentPlayer: Player; gameState: GameState }) {
    return (
        <div className="mb-4 bg-white dark:bg-gray-800 rounded-lg shadow-lg p-4">
            <div className="flex justify-between items-center">
                <div>
                    {isGameFinished && winner ? (
                        <div className="text-xl font-bold">
                            {winner.type === 'Bot' ? (
                                <span className="text-red-600 dark:text-red-400">🤖 Bot Wins!</span>
                            ) : (
                                <span className="text-green-600 dark:text-green-400">🎉 You Win!</span>
                            )}
                        </div>
                    ) : (
                        <>
                            <div className="text-lg font-semibold text-gray-900 dark:text-white">
                                {isPlayerTurn ? (
                                    <span className="text-green-600 dark:text-green-400">Your Turn</span>
                                ) : (
                                    <span className="text-orange-600 dark:text-orange-400">Bot is thinking...</span>
                                )}
                            </div>
                            <div className="text-sm text-gray-600 dark:text-gray-400">
                                Current Player: {currentPlayer?.name || 'Unknown'}
                            </div>
                        </>
                    )}
                </div>

                <div className="flex gap-6">
                    {gameState.players.map((player) => (
                        <div
                            key={player.id}
                            className={`text-center p-3 rounded-lg ${player.id === currentPlayer?.id && !isGameFinished
                                ? 'bg-indigo-100 dark:bg-indigo-900 ring-2 ring-indigo-500'
                                : 'bg-gray-100 dark:bg-gray-700'
                                }`}
                        >
                            <div className="text-sm font-semibold text-gray-900 dark:text-white">
                                {player.name} {player.type === 'Bot' ? '🤖' : '👤'}
                            </div>
                            <div className="text-xs text-gray-600 dark:text-gray-400">
                                Walls: {player.wallsRemaining}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}