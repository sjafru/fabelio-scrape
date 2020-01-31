import React, { Component } from 'react';
import { connect } from 'react-redux';
import { Button, Form, FormGroup, Label, Input, FormText, Container, Row, Col } from 'reactstrap';
import { RouteComponentProps } from 'react-router';
import * as fabelioProducts from '../store/FabelioProducts';
import { ApplicationState } from '../store';

type AddFabelioProductProps =  fabelioProducts.AddFabelioProductState // ... state we've requested from the Redux store
& typeof fabelioProducts.actionCreators // ... plus action creators we've requested
& RouteComponentProps<{ pageUrl: string }>; // ... plus incoming routing parameters

class AddFabelioProduct extends Component<AddFabelioProductProps> {
  constructor(props: any) {
    super(props);

    this.state = {
      pageUrl: 'Please paste the fabelio product url here.'
    };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  handleChange(event: any) {
    this.setState({ pageUrl: event.target.value });
  }

  handleSubmit(event: any) {
    alert('An essay was submitted: ' + this.props.productUrl);
    event.preventDefault();
  }

  render() {
    return (
      <Form onSubmit={this.handleSubmit}>
        <FormGroup>
          <Container>
            <Row>
              <Col><Label for="productUrl">Fabelio Product Url</Label></Col>
            </Row>
            <Row>
              <Col><Input required type="url" name="productUrl" onChange={this.handleChange} value={this.props.productUrl} id="productUrl" placeholder="put a fabelio.com product url" /></Col>
              <Col><Button>Submit</Button></Col>
            </Row>
          </Container>
        </FormGroup>
      </Form>
    );
  }
}

export default connect(
  (state: ApplicationState) => state.addFabelioProduct, // Selects which state properties are merged into the component's props
  fabelioProducts.actionCreators // Selects which action creators are merged into the component's props
)(AddFabelioProduct as any);
