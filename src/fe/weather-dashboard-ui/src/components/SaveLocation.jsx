import { useState } from "react";
import { Toast } from "react-bootstrap";
import { saveDefaultLocation } from "../services/apiClient";
import { useUser } from "../context/UserContext";

export default function SaveLocation({ weather }) {
    const { userId } = useUser();
    const [toast, setToast] = useState({ show: false, message: "", bg: "success" });

    if (!weather || !userId) return null;

    const handleSave = async () => {
        try {
            await saveDefaultLocation(userId, weather.location);
            setToast({ show: true, message: `Saved ${weather.location} as default location`, bg: "success" });
        } catch (err) {
            console.error("Failed to save default location", err);
            setToast({ show: true, message: "Could not save default location", bg: "danger" });
        }
    };

    return (
        <>
            <button className="btn btn-secondary mt-3" onClick={handleSave}>
                Save {weather.locationName} as your default location
            </button>

            <Toast
                bg={toast.bg}
                show={toast.show}
                onClose={() => setToast({ ...toast, show: false })}
                delay={3000}
                autohide
                className="mt-3"
            >
                <Toast.Body className="text-white">{toast.message}</Toast.Body>
            </Toast>
        </>
    );
}