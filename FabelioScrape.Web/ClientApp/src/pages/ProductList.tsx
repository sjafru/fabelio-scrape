import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import * as fabelioProducts from '../store/FabelioProducts';
import { ApplicationState } from '../store';
import ProductListItem from '../components/ProductListItem.component';
import { Link } from 'react-router-dom';

type ProductListProps = fabelioProducts.ListFabelioProductsState // ... state we've requested from the Redux store
    & typeof fabelioProducts.actionCreators // ... plus action creators we've requested
    & RouteComponentProps<{ page: string, size: string }>; // ... plus incoming routing parameters

class ProductList extends React.PureComponent<ProductListProps> {
    // This method is called when the component is first added to the document
    public componentDidMount() {
        this.ensureDataFetched();
    }

    // This method is called when the route parameters change
    public componentDidUpdate() {
        this.ensureDataFetched();
    }

    public render() {
        return (
            <React.Fragment>
                <h1 id="tabelLabel">Fabelio Products</h1>
                <p>This component demonstrates fetching data from the server and working with URL parameters.</p>
                {this.renderTable()}
            </React.Fragment>
        );
    }

    renderPagination(): React.ReactNode {
      const prevStartDateIndex = (this.props.page || 0) - 5;
        const nextStartDateIndex = (this.props.page || 0) + 5;
        const pageSize = (this.props.size || 0) + 25;

        return (
            <div className="d-flex justify-content-between">
                <Link className='btn btn-outline-secondary btn-sm' to={`/product-list/${prevStartDateIndex}/${pageSize}`}>Previous</Link>
                {this.props.isLoading && <span>Loading...</span>}
                <Link className='btn btn-outline-secondary btn-sm' to={`/product-list/${nextStartDateIndex}/${pageSize}`}>Next</Link>
            </div>
        );
    }

    renderTable(): React.ReactNode {
        return (
            <div className="d-flex p-2 justify-content-between">
                {(this.props.products || []).map((p: fabelioProducts.FabelioProduct) =>
                    <ProductListItem item={p} key={p.id}></ProductListItem>
                )}
            </div>
        );
    }

    private ensureDataFetched() {
        const page = parseInt(this.props.match.params.page, 10) || 1;
        const size = parseInt(this.props.match.params.size, 10) || 25;
        this.props.requestListProducts(page, size);
    }

}

export default connect(
    (state: ApplicationState) => state.listFabelioProduct, // Selects which state properties are merged into the component's props
    fabelioProducts.actionCreators // Selects which action creators are merged into the component's props
)(ProductList as any);