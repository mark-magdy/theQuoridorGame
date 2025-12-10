export default function AboutAndRules() {
    return (<div className="space-y-4 text-gray-700 dark:text-gray-300">
                    <section>
                        <h3 className="text-xl font-bold mb-2">🎯 Objective</h3>
                        <p>Be the first player to reach the opposite side of the board!</p>
                    </section>

                    <section>
                        <h3 className="text-xl font-bold mb-2">🎮 How to Play</h3>
                        <ul className="list-disc list-inside space-y-2">
                            <li>Each turn, you must either <strong>move your pawn</strong> or <strong>place a wall</strong></li>
                            <li>Pawns move one square orthogonally (up, down, left, right)</li>
                            <li>Pawns can jump over adjacent opponents if no wall blocks</li>
                            <li>Walls are 2 squares long and block movement between cells</li>
                            <li>Walls cannot completely block any player&apos;s path to their goal</li>
                        </ul>
                    </section>

                    <section>
                        <h3 className="text-xl font-bold mb-2">🧩 Game Setup</h3>
                        <ul className="list-disc list-inside space-y-2">
                            <li><strong>2 Players:</strong> 10 walls each, start at opposite sides</li>
                            <li><strong>3 Players:</strong> 7 walls each</li>
                            <li><strong>4 Players:</strong> 5 walls each, start on all four sides</li>
                        </ul>
                    </section>

                    <section>
                        <h3 className="text-xl font-bold mb-2">⌨️ Keyboard Shortcuts</h3>
                        <ul className="list-disc list-inside space-y-2">
                            <li><strong>Ctrl+Z:</strong> Undo</li>
                            <li><strong>Ctrl+Shift+Z:</strong> Redo</li>
                            <li><strong>Esc:</strong> Close dialogs</li>
                        </ul>
                    </section>

                    <section>
                        <h3 className="text-xl font-bold mb-2">💡 Strategy Tips</h3>
                        <ul className="list-disc list-inside space-y-2">
                            <li>Balance offense and defense - don&apos;t waste all walls too early</li>
                            <li>Force opponents to take longer paths</li>
                            <li>Watch out for opponents trying to trap you</li>
                            <li>Sometimes advancing your pawn is better than placing a wall</li>
                        </ul>
                    </section>

                    <section className="border-t border-gray-300 dark:border-gray-600 pt-4">
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                            Built with Next.js 14, TypeScript, TailwindCSS, and Framer Motion
                        </p>
                    </section>
                </div>); 
}