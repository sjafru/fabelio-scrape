import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface AddFabelioProductState {
    productUrl: string;
    replyMessage: string;
    isValid: boolean;
}

export interface ListFabelioProductsState {
    isLoading: boolean;
    page: number;
    size: number;
    products?: FabelioProduct[];
}

export interface DetailFabelioProductState {
    product: FabelioProduct;
    id: string;
}

export interface FabelioProduct {
    id: string;
    title: string;
    subTitle: string;
    images: string[];
    finalPrice: number;
    oldPrice: number;
    description: string;
}

export const DefaultFabelioProduct = {
    id: '',
    title: '',
    subTitle: '',
    images: [],
    finalPrice: 0,
    oldPrice: 0,
    description: ''
};

export interface ServerListReply {
    data?: FabelioProduct[],
    success: boolean,
    message: string,
    timestamp: string,
}

export interface ServerReply {
    data?: FabelioProduct,
    success: boolean,
    message: string,
    timestamp: string,
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

export interface ReqAddFabelioProductAction { type: 'REQ_ADD_FABELIO_PRODUCT', productUrl: string }
export interface ResAddFabelioProductAction { type: 'RES_ADD_FABELIO_PRODUCT', productUrl: string, product?: FabelioProduct, replyMessage: string, isValid: boolean }

interface ReqListFabelioProductsAction {
    type: 'REQ_LIST_FABELIO_PRODUCTS';
    page: number;
    size: number;
}

interface ResListFabelioProductsAction {
    type: 'RES_LIST_FABELIO_PRODUCTS';
    page: number;
    size: number;
    products?: FabelioProduct[];
}

interface ReqDetailFabelioProductAction {
    type: 'REQ_DETAIL_FABELIO_PRODUCT';
    id: string;
}

interface ResDetailFabelioProductAction {
    type: 'RES_DETAIL_FABELIO_PRODUCT';
    id: string;
    product?: FabelioProduct;
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = ReqAddFabelioProductAction | ResAddFabelioProductAction | ReqListFabelioProductsAction | ResListFabelioProductsAction | ReqDetailFabelioProductAction | ResDetailFabelioProductAction;


// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    addFabelioProduct: (productUrl: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.addFabelioProduct && productUrl !== appState.addFabelioProduct.productUrl) {
            fetch(`products?fabelioProductURL=` + productUrl, {
                method: 'post',
                body: JSON.stringify({})
            })
                .then(response => response.json() as Promise<ServerReply>)
                .then(data => {
                    dispatch({ type: 'RES_ADD_FABELIO_PRODUCT', productUrl: productUrl, product: data.data, isValid: data.success, replyMessage: data.message });
                });

            dispatch({ type: 'REQ_ADD_FABELIO_PRODUCT', productUrl: productUrl });
        }
    },

    requestListProducts: (page: number, size: number): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.listFabelioProduct && page !== appState.listFabelioProduct.page) {
            fetch(`products?page=${page - 1}&size=${size}`)
                .then(response => response.json() as Promise<ServerListReply>)
                .then(data => {
                    dispatch({ type: 'RES_LIST_FABELIO_PRODUCTS', page: page, size: size, products: data.data });
                });

            dispatch({ type: 'REQ_LIST_FABELIO_PRODUCTS', page: page, size: size });
        }
    },

    requestProductDetail: (id: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        const appState = getState();
        if (appState && appState.detailProduct && id !== appState.detailProduct.id) {
            fetch(`products/detail?id=${id}`)
                .then(response => response.json() as Promise<ServerReply>)
                .then(data => {
                    dispatch({ type: 'RES_DETAIL_FABELIO_PRODUCT', id: id, product: data.data });
                });

            dispatch({ type: 'REQ_DETAIL_FABELIO_PRODUCT', id: id });
        }
    }
};


// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const addFabelioProductReducer: Reducer<AddFabelioProductState> = (state: AddFabelioProductState | undefined, incomingAction: Action): AddFabelioProductState => {
    if (state === undefined) {
        return { productUrl: '', replyMessage: '', isValid: true };
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQ_ADD_FABELIO_PRODUCT':
            return { productUrl: state.productUrl, replyMessage: '', isValid: true };
        case 'RES_ADD_FABELIO_PRODUCT':
            return { productUrl: action.productUrl, replyMessage: action.replyMessage, isValid: action.isValid };
        default:
            return state;
    }
};

const unloadedState: ListFabelioProductsState = { products: [], page: 0, size: 25, isLoading: false };

export const listFabelioProductReducer: Reducer<ListFabelioProductsState> = (state: ListFabelioProductsState | undefined, incomingAction: Action): ListFabelioProductsState => {
    if (state === undefined) {
        return unloadedState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQ_LIST_FABELIO_PRODUCTS':
            return {
                page: action.page,
                size: action.size,
                products: state.products,
                isLoading: true
            };
        case 'RES_LIST_FABELIO_PRODUCTS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            if (action.page === state.page) {
                return {
                    page: action.page,
                    size: action.size,
                    products: action.products,
                    isLoading: false
                };
            }
            break;
    }

    return state;
};



export const detalFabelioProductReducer: Reducer<DetailFabelioProductState> = (state: DetailFabelioProductState | undefined, incomingAction: Action): DetailFabelioProductState => {
    if (state === undefined) {
        return { id: '', product: DefaultFabelioProduct };
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQ_DETAIL_FABELIO_PRODUCT':
            return { id: action.id, product: state.product };

        case 'RES_DETAIL_FABELIO_PRODUCT':
            return { id: action.id, product: (action.product || DefaultFabelioProduct) };
            break;
    }

    return state;
};