import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import SearchWeather from "./components/SearchWeather";
import { UserProvider } from "./context/UserContext";
import UserIdInfo from "./components/UserIdInfo";
import { ToastContainer } from "react-bootstrap";
import WeatherCard from "./components/WeatherCard";
import DefaultLocationWeather from "./components/DefaultLocationWeather";
import SaveLocation from "./components/SaveLocation";

export default function App() {
    const [weather, setWeather] = useState(null);
    const [isDefaultLocation, setIsDefaultLocation] = useState(null);
    
  return (
      <UserProvider>
      <div className="container py-4">
          <h1 className="text-center mb-4">Weather Dashboard 🌤️</h1>

          <div className="row">
              <div className="col-12">
                  <SearchWeather setWeather={setWeather} setIsDefaultLocation={setIsDefaultLocation} />
                  <DefaultLocationWeather setWeather={setWeather} setIsDefaultLocation={setIsDefaultLocation} />
                  
                  {weather && (
                      <WeatherCard weather={weather}/>
                  )}

                  {!isDefaultLocation && (
                      <SaveLocation weather={weather} />
                  )}

              </div>
          </div>
          <ToastContainer position="bottom-end" className="p-3" />
      </div>
      <UserIdInfo />
      </UserProvider>
  )
}
