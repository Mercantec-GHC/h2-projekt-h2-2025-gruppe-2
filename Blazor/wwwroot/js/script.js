window.loginHelpers = {
    saveToken: function (name, value) {
        console.log("Saving " + value + " as " + name);
        localStorage.setItem(name, value.toString());
    },

    loadItem: function (name) {
        console.log("Loading " + name);
        return localStorage.getItem(name) ?? "";
    },
    
    deleteItem: function (name) {
        localStorage.removeItem(name);
    },
    
    saveItemToSession: function (name, value) {
        sessionStorage.setItem(name, value.toString());
    },
    
    getItemFromSession: function (name, value) {
        sessionStorage.getItem(name);
    }
};