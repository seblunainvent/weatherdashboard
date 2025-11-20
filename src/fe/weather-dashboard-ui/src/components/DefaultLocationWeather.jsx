import { useEffect } from "react";
import { getDefaultLocation, getWeatherByLocation } from "../services/apiClient";
import { useUser } from "../context/UserContext";

export default function DefaultLocationWeather({ setWeather, setIsDefaultLocation }) {
    const { userId } = useUser();
    
    useEffect(() => {
        const fetchDefaultWeather = async () => {
            if (!userId) return;
            try {
                const location = await getDefaultLocation(userId);
                
                if (location.locationName) {
                    const data = await getWeatherByLocation(location.locationName);
                    setWeather(data);
                    setIsDefaultLocation(true);
                }
            } catch (err) {
                console.log(err);
                console.log("No default location set");
            }
        };
        fetchDefaultWeather();
    }, [userId, setWeather]);

    return null;
}