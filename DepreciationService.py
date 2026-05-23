from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route('/depreciation/calculate', methods=['POST'])
def calculate_depreciation():

    data = request.json

    cost = data['cost']
    useful_life = data['useful_life']

    depreciation = cost / useful_life

    return jsonify({
        "yearly_depreciation": depreciation
    })

if __name__ == '__main__':
    app.run(port=5002)