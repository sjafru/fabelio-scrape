import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom';
import { Card, CardImg, CardBody, CardTitle, CardSubtitle, CardText, Button } from 'reactstrap';
import * as fabelioProducts from '../store/FabelioProducts';
import { ApplicationState } from '../store';
import ImageCarrousel from '../components/ImageCarrousel.component';

type ProductDetailProps = fabelioProducts.DetailFabelioProductState // ... state we've requested from the Redux store
    & typeof fabelioProducts.actionCreators // ... plus action creators we've requested
    & RouteComponentProps<{ id: string }>; // ... plus incoming routing parameters

const styles = {
    divCard: {
        maxWidth: '720px',
        marginBottom: '10px'
    }
};

class ProductDetail extends React.PureComponent<ProductDetailProps> {
    // This method is called when the component is first added to the document
    public componentDidMount() {
        this.ensureDataFetched();
    }

    // This method is called when the route parameters change
    public componentDidUpdate() {
        this.ensureDataFetched();
    }

    private ensureDataFetched() {
        this.props.requestProductDetail(this.props.match.params.id);
    }

    render() {
        return (
            <React.Fragment>
                {this.renderDetail()}
            </React.Fragment>
        );
    }

    renderDetail(): React.ReactNode {
        return (
            <div style={styles.divCard} className="m-2">
                <Link to="/product-list" className="btn btn-primary">Back</Link>
                <Card>
                    <ImageCarrousel items={this.props.product.images.map((v) => {
                        return { src: v, caption: "", altText: "" };
                    })}></ImageCarrousel>
                    <CardBody>
                        <CardTitle>{this.props.product.title}</CardTitle>
                        <CardSubtitle>{this.props.product.subTitle}</CardSubtitle>
                        <CardText dangerouslySetInnerHTML={{ __html: this.props.product.description }}></CardText>
                    </CardBody>
                </Card>
            </div>
        );
    }
}

export default connect(
    (state: ApplicationState) => state.detailProduct, // Selects which state properties are merged into the component's props
    fabelioProducts.actionCreators // Selects which action creators are merged into the component's props
)(ProductDetail as any)
