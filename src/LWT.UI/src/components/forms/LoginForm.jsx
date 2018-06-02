import React from 'react';
import { Form, Button, Label } from 'semantic-ui-react';
import Validator from 'validator';
import PropTypes from 'prop-types';
import InlineError from '../messages/InlineError';

class LoginForm extends React.Component {
  state = {
    data: {
      userName: '',
      password: '',
    },
    errors: {},
  }

  onChange = e => this.setState({
    data: {
      ...this.state.data,
      [e.target.name]: e.target.value,
    },
  })

  onSubmit = () => {
    const errors = this.validate(this.state.data);
    this.setState({ errors });
    if (Object.keys(errors).length === 0) {
      this.props.submit(this.state.data);
    }
  }

  validate=(data) => {
    const errors = {};
    if (!Validator.isLength(data.userName, 4)) {
      errors.userName = 'Invalid user name';
    }
    if (!Validator.isLength(data.password, 6, 20)) {
      errors.password = 'Invalid password';
    }
    return errors;
  }

  render() {
    const { data, errors } = this.state;
    return (
      <Form onSubmit={this.onSubmit} >
        <Form.Field error={!!errors.userName}>
          <Label htmlFor="userName">UserName</Label>
          <input
            // type="userName"
            name="userName"
            id="userName"
            placeholder="example@example.com"
            value={data.userName}
            onChange={this.onChange}
          />
          {errors.userName && <InlineError error={errors.userName} />}
        </Form.Field>
        <Form.Field error={!!errors.password}>
          <Label htmlFor="password">Password</Label>
          <input
            type="password"
            name="password"
            id="password"
            placeholder="Enter your password"
            value={data.password}
            onChange={this.onChange}
          />
          {errors.password && <InlineError error={errors.password} />}
        </Form.Field>

        <Button primary>Login</Button>
      </Form>
    );
  }
}

LoginForm.propTypes = {
  submit: PropTypes.func.isRequired,
};

export default LoginForm;
