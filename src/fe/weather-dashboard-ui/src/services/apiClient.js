import axios from "axios";

const api = axios.create({
    baseURL: "http://localhost:5177/api",
    timeout: 5000,
});

// Get weather by location name
export const getWeatherByLocation = async (locationName) => {
    const res = await api.get("weather", { params: { locationName } });
    return res.data;
};

// Get weather for default location
export const getDefaultLocation = async (userId) => {
    const response = await api.get(`/user/location?userId=${userId}`);
    return response.data;
};

// Save default location
export const saveDefaultLocation = async (userId, locationName) => {
    await api.post(`/user/location?userId=${userId}`, {
        locationName: locationName,
    });
};