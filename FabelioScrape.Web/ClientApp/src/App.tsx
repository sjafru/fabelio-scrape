import * as React from 'react';
import { Route } from 'react-router';
import Layout from './pages/Layout';
import Home from './pages/Home';
import Counter from './pages/Counter';
import FetchData from './pages/FetchData';

import './custom.css'

export default () => (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetch-data/:startDateIndex?' component={FetchData} />
    </Layout>
);
