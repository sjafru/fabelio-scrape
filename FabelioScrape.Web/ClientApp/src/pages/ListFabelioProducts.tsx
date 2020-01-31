import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import * as fabelioProducts from '../store/FabelioProducts';
import { ApplicationState } from '../store';

type ListFabelioProductsProps = fabelioProducts.ListFabelioProductsState // ... state we've requested from the Redux store
    & typeof fabelioProducts.actionCreators // ... plus action creators we've requested
    & RouteComponentProps<{ startDateIndex: string }>; // ... plus incoming routing parameters

class ListFabelioProducts extends React.PureComponent<ListFabelioProductsProps> {
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
                <h1 id="tabelLabel">Weather forecast</h1>
                <p>This component demonstrates fetching data from the server and working with URL parameters.</p>
                {this.renderTable()}
                {this.renderPagination()}
            </React.Fragment>
        );
    }
    
    renderPagination(): React.ReactNode {
        throw new Error("Method not implemented.");
    }
    
    renderTable(): React.ReactNode {
        throw new Error("Method not implemented.");
    }

    private ensureDataFetched() {
        const startDateIndex = parseInt(this.props.match.params.startDateIndex, 10) || 0;
        this.props.requestListProducts(startDateIndex);
    }

}

export default connect(
    (state: ApplicationState) => state.listFabelioProduct, // Selects which state properties are merged into the component's props
    fabelioProducts.actionCreators // Selects which action creators are merged into the component's props
)(ListFabelioProducts as any);