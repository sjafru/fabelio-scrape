import * as React from 'react';
import { connect } from 'react-redux';
import AddFabelioProduct from '../components/AddProduct.component';

const Home = () => (
  <div>
    <h1>Welcome to Fabelio Scraper Page</h1>
    <ul>
      <li>Product URL should get from <a href='https://www.fabelio.com/' rel="noopener noreferrer" target="_blank">www.fabelio.com</a></li>
    </ul>

    <AddFabelioProduct></AddFabelioProduct>

  </div>
);

export default connect()(Home);
