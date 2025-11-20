import { useState } from "react";
import { getWeatherByLocation } from "../services/apiClient";
import WeatherCard from "./WeatherCard";
import SaveLocation from "./SaveLocation";

export default function SearchWeather({setWeather, setIsDefaultLocation}) {
    const [location, setLocation] = useState("");
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    const handleSearch = async (e) => {
        e.preventDefault();
        if (!location.trim()) return;

        setLoading(true);
        setError(null);
        setWeather(null);
        setIsDefaultLocation(false);

        try {
            const data = await getWeatherByLocation(location);
            setWeather(data);
        } catch (err) {
            if (err.response?.status === 404) {
                setError("Location not found. Try another city.");
            } else {
                setError("Could not fetch weather data.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div>
            <form className="d-flex mb-3" onSubmit={handleSearch}>
                <div className="input-group mb-3">
                    <input type="text" className="form-control" placeholder="Enter location..." 
                           value={location} 
                           onChange={(e) => setLocation(e.target.value)}
                           aria-label="Recipient’s username" aria-describedby="button-addon2"/>
                    <button className="btn btn-primary" type="submit" id="button-addon2">Get Weather</button>
                </div>
            </form>

            {loading && (
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            )}

            {error && <div className="alert alert-danger">{error}</div>}
        </div>
    );
}