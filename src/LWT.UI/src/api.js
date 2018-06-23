import axios from "axios";

export default {
  user: {
    login: credentails => axios.post("/user/login", credentails)
  }
};
