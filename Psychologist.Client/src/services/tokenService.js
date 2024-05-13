class TokenService {
    checkTokens() {
        const auth = JSON.parse(localStorage.getItem("auth"));
        return !!auth && !!auth?.accessToken && !!auth?.refreshToken;
    }
    
    getTokens() {
        const auth = JSON.parse(localStorage.getItem("auth"));
        return { accessToken: auth?.accessToken, refreshToken: auth?.refreshToken };
    }

    updateTokens({ accessToken, refreshToken }) {
        let auth = JSON.parse(localStorage.getItem("auth")) ?? {};
        auth.accessToken = accessToken;
        auth.refreshToken = refreshToken;
        localStorage.setItem("auth", JSON.stringify(auth));
    }

    removeTokens() {
        localStorage.removeItem("auth");
    }
}

export default new TokenService();