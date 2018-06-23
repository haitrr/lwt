import React from 'react';
import { PropTypes } from 'prop-types';

const InlineError = ({ error }) => (
  <span style={{ color: 'red' }}>{error}</span>
);

InlineError.propTypes = {
  error: PropTypes.string.isRequired,
};

export default InlineError;
