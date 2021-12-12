from natto import MeCab
from flask import Flask, request, jsonify

app = Flask(__name__)
app.config["DEBUG"] = True
nm = MeCab()


@app.route('/cut', methods=['POST'])
def cut():
    data = request.json
    rs = []
    for n in nm.parse(data["text"], as_nodes=True):
        rs.append(n.surface)
    return jsonify(rs)
        

app.config['JSON_AS_ASCII'] = False
app.run(port=5032)