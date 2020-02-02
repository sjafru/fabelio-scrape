import React, { Component } from 'react';
import { connect } from 'react-redux';
import { Button, Form, FormGroup, Label, Input, Container, Row, Col } from 'reactstrap';
import { RouteComponentProps } from 'react-router';
import * as fabelioProducts from '../store/FabelioProducts';
import { ApplicationState } from '../store';

type AddProductProps = fabelioProducts.AddFabelioProductState // ... state we've requested from the Redux store
  & typeof fabelioProducts.actionCreators // ... plus action creators we've requested
  & RouteComponentProps<{ pageUrl: string }>; // ... plus incoming routing parameters

class AddProduct extends Component<AddProductProps, fabelioProducts.AddFabelioProductState> {
  constructor(props: any) {
    super(props);

    this.state = { productUrl: "" };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
  }

  handleChange(event: any) {
    this.setState({ productUrl: event.target.value });
  }

  handleSubmit(event: any) {
    this.props.addFabelioProduct(this.state.productUrl);
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
              <Col><Input required type="url" name="url" onChange={this.handleChange} value={this.state.productUrl} id="productUrl" placeholder="put a fabelio.com product url" /></Col>
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
)(AddProduct as any);
