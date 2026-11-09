import axios from 'axios';
axios.defaults.baseURL="http://localhost:5181"

axios.interceptors.response.use(
  response => response,          
  error => {                    
    console.log("שגיאה מהשרת:", error);
    return Promise.reject(error);
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
  }
};
