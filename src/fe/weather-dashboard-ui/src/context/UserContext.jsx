import { createContext, useContext, useState } from "react";

const UserContext = createContext({ userId: null });

const getOrCreateUserId = () => {
    let userId = localStorage.getItem("userId");
    if (!userId) {
        userId = crypto.randomUUID();
        localStorage.setItem("userId", userId);
    }
    return userId;
};

export const UserProvider = ({ children }) => {
    const [userId] = useState(getOrCreateUserId());

    return (
        <UserContext.Provider value={{ userId }}>
            {children}
        </UserContext.Provider>
    );
};

export const useUser = () => useContext(UserContext);