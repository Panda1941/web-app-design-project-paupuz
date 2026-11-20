import { Link, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { Panel } from './components/common'
import { ChampionsPage } from './pages/ChampionsPage'
import { ChampionPage } from './pages/ChampionPage'
import { BuildPage } from './pages/BuildPage'
import { LoginPage, RegisterPage } from './pages/AuthPages'

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<ChampionsPage />} />
        <Route path="/champions/:championId" element={<ChampionPage />} />
        <Route path="/champions/:championId/builds/:buildId" element={<BuildPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="*"
          element={
            <Panel title="Page not found">
              <p>
                This page doesn't exist. <Link to="/">Back to champions</Link>.
              </p>
            </Panel>
          }
        />
      </Routes>
    </Layout>
  )
}
