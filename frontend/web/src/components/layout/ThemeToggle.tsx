import { useTheme } from '../../theme/ThemeProvider'

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme()

  return (
    <button
      type="button"
      className="theme-toggle-btn"
      onClick={toggleTheme}
      title={theme === 'dark' ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
      aria-label={theme === 'dark' ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
    >
      <i className={`bi ${theme === 'dark' ? 'bi-sun-fill text-warning' : 'bi-moon-stars-fill'}`} />
    </button>
  )
}
