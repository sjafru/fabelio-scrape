import * as React from 'react';
import { Card, CardImg, CardBody, CardTitle, CardSubtitle, CardText, Button } from 'reactstrap';
import { connect } from 'react-redux';
import { FabelioProduct } from '../store/FabelioProducts';

type ProductListItemProps = {
    item: FabelioProduct,
    key: string
};

const ProductListItem = (props: ProductListItemProps) => (
    <div>
        <Card>
            <CardImg top width="100%" src={props.item.imageUrls[0]} alt="Card image cap" />
            <CardBody>
                <CardTitle>{props.item.title}</CardTitle>
                <CardSubtitle>{props.item.subTitle}</CardSubtitle>
                <CardText dangerouslySetInnerHTML={{ __html: props.item.description }}></CardText>
                <Button>Detail</Button>
            </CardBody>
        </Card>
    </div>
);

export default connect()(ProductListItem);
