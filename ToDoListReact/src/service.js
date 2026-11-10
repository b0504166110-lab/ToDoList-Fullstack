import axios from 'axios';
axios.defaults.baseURL="http://localhost:5181"

axios.interceptors.response.use(
  response => response,          
  error => {                    
    console.log("שגיאה מהשרת:", error);
    // 401 = אין טוקן, או שהוא פג או לא תקין. לא כולל /login עצמו,
    // שם 401 פירושו סיסמה שגויה, ו-Login.js מציג הודעה.
    if (error.response?.status === 401 && !error.config.url.endsWith("/login")) {
      localStorage.removeItem("token");
      window.location.reload(); // אחרי הרענון App לא מוצא טוקן ומציג את מסך ההתחברות
    }
    return Promise.reject(error);
  }
);
axios.interceptors.request.use(
 config=>{
  const token=localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
 }          
  
);

export default {
  getTasks: async () => {
    const result = await axios.get(`/items`)    
    return result.data;
  },

  addTask: async(name)=>{
  const result = await axios.post(`/items`, { name, isComplete: false })
  return result.data;
},

  setCompleted: async(id, isComplete)=>{
    console.log('setCompleted', {id, isComplete})
  const result = await axios.put(`/items/${id}`, {isComplete: isComplete })
  return result.data;
  },

  deleteTask:async(id)=>{
    console.log('deleteTask')
    await axios.delete(`/items/${id}`)
  },
    register: async (userName, password) => {
    await axios.post(`/register`, { userName, password });
  },

  login: async (userName, password) => {
    const result = await axios.post(`/login`, { userName, password });
    localStorage.setItem("token", result.data.token);
  },

  logout: () => {
    localStorage.removeItem("token");
  }
};
