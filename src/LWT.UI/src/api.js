import axios from "axios";

export default {
  user: {
    login: credentails =>
      axios.post("/user/auth", credentails).then(res => res.data.user)
  }
};
