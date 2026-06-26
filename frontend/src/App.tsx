import { Routes, Route } from 'react-router-dom'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<div className="p-8 text-center"><h1 className="text-2xl font-bold">AlpineGearHub</h1></div>} />
    </Routes>
  )
}
