import { useUser } from "../context/UserContext";

export default function UserIdInfo() {
    const { userId } = useUser();

    return (
        <small className="small text-center text-muted mb-3">
            Your User ID: <strong>{userId}</strong>
        </small>
    );
}