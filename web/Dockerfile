FROM node:lts-buster-slim as build-stage
WORKDIR /app

COPY dist.zip ./
# Replace the default Debian repository with the Tsinghua University mirror
# RUN echo "deb http://mirrors.tuna.tsinghua.edu.cn/debian/ buster main contrib non-free" > /etc/apt/sources.list \
#  && echo "deb http://mirrors.tuna.tsinghua.edu.cn/debian/ buster-updates main contrib non-free" >> /etc/apt/sources.list \
#  && echo "deb http://mirrors.tuna.tsinghua.edu.cn/debian/ buster-backports main contrib non-free" >> /etc/apt/sources.list \
#  && echo "deb http://mirrors.tuna.tsinghua.edu.cn/debian-security buster/updates main contrib non-free" >> /etc/apt/sources.list

RUN apt-get update && apt-get install -y unzip
RUN unzip dist.zip && rm dist.zip
COPY conf/default.conf ./

# COPY package*.json ./

# RUN yarn config set registry https://registry.npm.taobao.org
# RUN yarn config set sass_binary_site http://cdn.npm.taobao.org/dist/node-sass
# RUN yarn
# RUN yarn add react-scripts@4.0.3


# RUN rm -rf node_modules
# RUN npm cache clear --force

# RUN yarn install
COPY . .
# RUN yarn build

#production stage
FROM nginx:stable-alpine as production-stage
COPY --from=0 /app/dist /usr/share/nginx/html
COPY conf/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]