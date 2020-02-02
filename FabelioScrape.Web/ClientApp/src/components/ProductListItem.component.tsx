import * as React from 'react';
import { Link } from 'react-router-dom';
import { connect } from 'react-redux';
import { Card, CardImg, CardBody, CardTitle, CardSubtitle, CardText, Button } from 'reactstrap';
import { FabelioProduct } from '../store/FabelioProducts';


type ProductListItemProps = {
    item: FabelioProduct,
    key: string
};

const styles = {
    divCard: {
        marginBottom: '10px',
        maxWidth: '400px'
    }
};

const ProductListItem = (props: ProductListItemProps) => (
    <div style={styles.divCard}>
        <Card>
            <CardImg top width="100%" src={props.item.images[0]} alt="Card image cap" />
            <CardBody>
                <CardTitle>{props.item.title}</CardTitle>
                <CardSubtitle>{props.item.subTitle}</CardSubtitle>
                <CardText>{props.item.oldPrice}</CardText>
                <CardText>{props.item.finalPrice}</CardText>
                <Link className="btn btn-info" to={`/product-detail/${props.item.id}`}>Detail</Link>
            </CardBody>
        </Card>
    </div>
);

export default connect()(ProductListItem);
