# -*- coding: utf-8 -*-
"""
Created on Thu Apr 11 10:21:57 2019

@author: yzdnpx
"""

import pandas as pd
import os
import numpy as np
import time

start = time.time()
from sklearn.preprocessing import LabelEncoder, OneHotEncoder

workPath = 'V:/Naveen/Learning/RedHat'

dataset = pd.read_csv(os.path.join(workPath, 'dataset.csv'))

X = dataset.iloc[:, :-1]

y = dataset.iloc[:, 2]

labelencoder1 = LabelEncoder()

X.iloc[:,0] = labelencoder1.fit_transform(X.iloc[:,0])

labelencoder2 = LabelEncoder()

X.iloc[:,1] = labelencoder2.fit_transform(X.iloc[:,1])

onehotencoder1 = OneHotEncoder(categorical_features=[0])

onehotencoder1.fit(X)
X = onehotencoder1.transform(X).toarray()

#onehotencoder1.inverse_transform(X)

from sklearn.linear_model import LogisticRegression
LogReg_classifier = LogisticRegression(random_state = 0)
LogReg_classifier.fit(X, y)

X_CSV = pd.read_csv(os.path.join(workPath, 'test.csv'))

X_test = pd.read_csv(os.path.join(workPath, 'test.csv'))

labelencoder3 = LabelEncoder()

X_test.iloc[:,0] = labelencoder3.fit_transform(X_test.iloc[:,0])

labelencoder4 = LabelEncoder()

X_test.iloc[:,1] = labelencoder4.fit_transform(X_test.iloc[:,1])

onehotencoder2 = OneHotEncoder(categorical_features=[0])

X_test = onehotencoder2.fit_transform(X_test).toarray()

if X.shape[1] != X_test.shape[1] &  X.shape[1] > X_test.shape[1]:    
    zeros = np.zeros((X_test.shape[0], X.shape[1] - X_test.shape[1]))

    X_test = np.concatenate((X_test, zeros), axis=1)

predY = LogReg_classifier.predict(X_test)

predY = pd.DataFrame(predY)

X_test = pd.DataFrame(X_test)

pred_Test = pd.concat([X_CSV, predY], axis = 1)

pred_Test.to_csv(os.path.join(workPath, 'testPred.csv'))

print(time.time() - start)



