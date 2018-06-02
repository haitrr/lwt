import React from 'react';
import PropTypes from 'prop-types';
import LoginForm from '../forms/LoginForm';

class LoginPage extends React.Component {
  state = { }

  submit = (data) => {
    this.props.login(data).then(() => this.props.history.push('/'));
  }
  render() {
    return (
      <div>
        <h1>Login Page</h1>
        <LoginForm submit={this.submit} />
      </div>
    );
  }
}

LoginPage.propTypes = {
  login: PropTypes.func.isRequired,
  history: PropTypes.shape({
    push: PropTypes.func.isRequired,
  }).isRequired,
};

export default LoginPage;
