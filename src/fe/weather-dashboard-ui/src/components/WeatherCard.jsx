export default function WeatherCard({ weather }) {
    if (!weather) return null;

    const w = weather;

    return (
        <div className="card mb-3">
            <div className="row g-0">
                <div className="col-md-4">
                    <img
                        src={`${w.weather.iconUrl}`}
                        alt={w.weather.description}
                        className="mb-3"
                    />
                </div>
                <div className="col-md-8">
                    <div className="card-body">
                        <h4 className="card-title text-capitalize">{w.location}</h4>
                        <small className="text-muted">Lon : {w.longitude}, Lat : {w.latitude}</small>
                        <p className="card-text">{w.weather.description}</p>
                        <ul className="list-group list-group-flush text-start">    
                            <li className="list-group-item">🌡️ <strong>{w.weather.temperatureCelsius} °C</strong></li>
                            <li className="list-group-item">💧 {w.weather.humidityPercent}% humidity</li>
                            <li className="list-group-item">🌬️ {w.weather.windSpeedKph} km/h wind</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    );
}